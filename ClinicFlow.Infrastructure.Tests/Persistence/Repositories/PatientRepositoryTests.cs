using AwesomeAssertions;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Services.Contexts.Registration;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Persistence.Repositories;
using ClinicFlow.Infrastructure.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Infrastructure.Tests.Persistence.Repositories;

public class PatientRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly PatientRepository _sut = new(fixture.Context);
    private ApplicationDbContext Context => fixture.Context;

    public async ValueTask InitializeAsync()
    {
        await fixture.Respawner.ResetAsync(fixture.DbConnection);

        fixture.Context.ChangeTracker.Clear();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task CreateAsync_ShouldAddPatientToContext()
    {
        // Arrange
        var patient = Patient.CreateProfile(
            PersonName.Create("New", null, "Patient"),
            new DateOnly(1990, 1, 1),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetLocalNow().DateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(
            EmergencyContact.Create("Contact", null, "Contact", "555-9999")
        );

        // Act
        await _sut.CreateAsync(patient, TestContext.Current.CancellationToken);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var dbResult = await Context
            .Patients.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == patient.Id, TestContext.Current.CancellationToken);

        dbResult.Should().BeEquivalentTo(patient);
    }

    [Fact]
    public async Task CreateAsync_ShouldNotReAdd_WhenEntityIsAlreadyTracked()
    {
        // Arrange
        var patient = Patient.CreateProfile(
            PersonName.Create("New", null, "Patient"),
            new DateOnly(1990, 1, 1),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetLocalNow().DateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(
            EmergencyContact.Create("Contact", null, "Contact", "555-9999")
        );

        Context.Patients.Add(patient);
        Context.Entry(patient).State = EntityState.Unchanged;

        // Act
        await _sut.CreateAsync(patient, TestContext.Current.CancellationToken);

        // Assert
        Context.Entry(patient).State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPatient_WhenExistsAndActive()
    {
        // Arrange
        var patient = await CreateSelfPatientAsync();

        // Act
        var result = await _sut.GetByIdAsync(patient.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(patient);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.CreateVersion7();

        // Act
        var result = await _sut.GetByIdAsync(nonExistentId, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFuzzyMatchesBySearchNameAsync_ShouldReturnCandidateWithAccountContact_WhenNameAndDateOfBirthMatch()
    {
        // Arrange
        var patient = await CreatePatientAsync("John", "Doe");
        var user = await CreateAccountAsync(patient.Id);

        var expectedCandidate = CreateExpectedCandidate(patient, user);

        // Act
        var result = await _sut.GetFuzzyMatchesBySearchNameAsync(
            patient.FullName.SearchName,
            patient.DateOfBirth,
            ssnLast4Hash: null,
            TestContext.Current.CancellationToken
        );
        // Assert
        result.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedCandidate);
    }

    [Fact]
    public async Task GetFuzzyMatchesBySearchNameAsync_ShouldReturnEmpty_WhenDateOfBirthDiffers()
    {
        // Arrange
        var patient = await CreatePatientAsync("John", "Doe");

        // Act
        var result = await _sut.GetFuzzyMatchesBySearchNameAsync(
            patient.FullName.SearchName,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime),
            ssnLast4Hash: null,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFuzzyMatchesBySearchNameAsync_ShouldReturnEmpty_WhenSimilarityIsBelowThreshold()
    {
        // Arrange
        // "mary smith" shares no trigram with "john doe", so its similarity is 0.
        var patient = await CreatePatientAsync("Mary", "Smith");

        // Act
        var result = await _sut.GetFuzzyMatchesBySearchNameAsync(
            "john doe",
            patient.DateOfBirth,
            ssnLast4Hash: null,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFuzzyMatchesBySearchNameAsync_ShouldReturnCandidate_WhenSimilarityEqualsThreshold()
    {
        // Arrange
        // "jo do" has 6 and "jo bob" has 7, with 3 in common ("  j", " jo", "jo ").
        // similarity = 3 / (6 + 7 - 3) = 0.30, exactly the minimum similarity.
        var patient = await CreatePatientAsync("Jo", "Do");

        // Act
        var result = await _sut.GetFuzzyMatchesBySearchNameAsync(
            "jo bob",
            patient.DateOfBirth,
            ssnLast4Hash: null,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().ContainSingle().Which.PatientId.Should().Be(patient.Id);
    }

    [Fact]
    public async Task GetFuzzyMatchesBySearchNameAsync_ShouldReturnCandidatesOrderedBySimilarityDescending()
    {
        // Arrange
        // Similarity against "john doe": "john doe" = 1.00, "john doa" = 0.64, "john dan" = 0.50.
        // They are inserted in a different order so neither insertion order nor ascending order passes.
        var partialMatch = await CreatePatientAsync("John", "Doa");
        var weakMatch = await CreatePatientAsync("John", "Dan");
        var exactMatch = await CreatePatientAsync("John", "Doe");

        // Act
        var result = await _sut.GetFuzzyMatchesBySearchNameAsync(
            exactMatch.FullName.SearchName,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-67)),
            ssnLast4Hash: null,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .Should()
            .BeEquivalentTo(
                [
                    CreateExpectedCandidateWithoutContact(exactMatch),
                    CreateExpectedCandidateWithoutContact(partialMatch),
                    CreateExpectedCandidateWithoutContact(weakMatch),
                ],
                options => options.WithStrictOrdering()
            );
    }

    [Fact]
    public async Task GetFuzzyMatchesBySearchNameAsync_ShouldReturnAtMostTwentyCandidates()
    {
        // Arrange
        await CreatePatientsAsync(count: 21, "John", "Doe");

        // Act
        var result = await _sut.GetFuzzyMatchesBySearchNameAsync(
            "john doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-67)),
            ssnLast4Hash: null,
            TestContext.Current.CancellationToken
        );

        // Assert
        result.Should().HaveCount(20);
    }

    [Fact]
    public async Task GetFuzzyMatchesBySearchNameAsync_ShouldReturnCandidateWithoutContact_WhenPatientHasNoAccount()
    {
        // Arrange
        // An unrelated user exists, so a contact lookup without the membership filter would pick it up.
        var patient = await CreatePatientAsync("John", "Doe");
        var expectedCandidateWithoutContact = CreateExpectedCandidateWithoutContact(patient);

        await CreatePendingUserAsync();

        // Act
        var result = await _sut.GetFuzzyMatchesBySearchNameAsync(
            patient.FullName.SearchName,
            patient.DateOfBirth,
            ssnLast4Hash: null,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(expectedCandidateWithoutContact);
    }

    [Fact]
    public async Task GetFuzzyMatchesBySearchNameAsync_ShouldReturnCandidateWithoutContact_WhenOnlyNonSelfMembershipExists()
    {
        // Arrange
        var patient = await CreatePatientAsync("John", "Doe");
        var pendingUser = await CreatePendingUserAsync();
        var membership = FamilyMembership.CreateAdultFamilyMember(
            patient.Id,
            pendingUser.Id,
            PatientRelationship.Parent,
            FamilyMembershipAccessLevel.Full,
            FamilyMembership.MinimumAdultAge,
            _fakeTime.GetUtcNow()
        );

        var expectedCandidateWithoutContact = CreateExpectedCandidateWithoutContact(patient);

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetFuzzyMatchesBySearchNameAsync(
            patient.FullName.SearchName,
            patient.DateOfBirth,
            ssnLast4Hash: null,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(expectedCandidateWithoutContact);
    }

    [Fact]
    public async Task GetFuzzyMatchesBySearchNameAsync_ShouldReturnCandidateWithoutContact_WhenSelfMembershipIsNotActive()
    {
        // Arrange
        var patient = await CreatePatientAsync("John", "Doe");
        var user = await CreatePendingUserAsync();
        var membership = FamilyMembership.CreateSelf(
            patient.Id,
            user.Id,
            _fakeTime.GetUtcNow()
        );

        var expectedCandidateWithoutContact = CreateExpectedCandidateWithoutContact(patient);

        membership.CloseSelfMembership(_fakeTime.GetUtcNow().AddHours(1));

        Context.FamilyMemberships.Add(membership);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sut.GetFuzzyMatchesBySearchNameAsync(
            patient.FullName.SearchName,
            patient.DateOfBirth,
            ssnLast4Hash: null,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(expectedCandidateWithoutContact);
    }

    [Fact]
    public async Task GetFuzzyMatchesBySearchNameAsync_ShouldResolveEachContactFromItsOwnSelfMembership()
    {
        // Arrange
        // Two matching patients, each with its own account, so a contact filter that ignores
        // the user, the patient, the role or the status resolves the wrong user for one of them.
        var patient1 = await CreatePatientAsync("John", "Doe");
        var patient2 = await CreatePatientAsync("John", "Doa");
        var user1 = await CreateAccountAsync(patient1.Id);
        var user2 = await CreateAccountAsync(patient2.Id);

        // Act
        var result = await _sut.GetFuzzyMatchesBySearchNameAsync(
            patient1.FullName.SearchName,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-67)),
            ssnLast4Hash: null,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .Should()
            .BeEquivalentTo([
                CreateExpectedCandidate(patient1, user1),
                CreateExpectedCandidate(patient2, user2),
            ]);
    }

    [Fact]
    public async Task GetFuzzyMatchesBySearchNameAsync_ShouldReturnSsnLast4MatchesTrue_WhenHashMatches()
    {
        // Arrange
        var patient = await CreateHashedPatientAsync();
        var expectedCandidateWithoutContact = CreateExpectedCandidateWithoutContact(
            patient,
            ssnLast4Matches: true
        );

        // Act
        var result = await _sut.GetFuzzyMatchesBySearchNameAsync(
            patient.FullName.SearchName,
            patient.DateOfBirth,
            patient.SsnLast4Hash,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(expectedCandidateWithoutContact);
    }

    [Fact]
    public async Task GetFuzzyMatchesBySearchNameAsync_ShouldReturnSsnLast4MatchesFalse_WhenHashDoesNotMatch()
    {
        // Arrange
        var patient = await CreateHashedPatientAsync();
        var expectedCandidateWithoutContact = CreateExpectedCandidateWithoutContact(
            patient,
            ssnLast4Matches: false
        );

        // Act
        var result = await _sut.GetFuzzyMatchesBySearchNameAsync(
            patient.FullName.SearchName,
            patient.DateOfBirth,
            "fedcba9876543321fedcba9876543321fedcba9876543321fedcba9876543321",
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(expectedCandidateWithoutContact);
    }

    [Fact]
    public async Task GetFuzzyMatchesBySearchNameAsync_ShouldReturnSsnLast4MatchesFalse_WhenHashIsNullAndPatientHasNoHash()
    {
        // Arrange
        var patient = await CreateSelfPatientAsync();
        var expectedCandidateWithoutContact = CreateExpectedCandidateWithoutContact(
            patient,
            ssnLast4Matches: false
        );

        // Act
        var result = await _sut.GetFuzzyMatchesBySearchNameAsync(
            patient.FullName.SearchName,
            patient.DateOfBirth,
            ssnLast4Hash: null,
            TestContext.Current.CancellationToken
        );

        // Assert
        result
            .Should()
            .ContainSingle()
            .Which.Should()
            .BeEquivalentTo(expectedCandidateWithoutContact);
    }

    private static PatientDuplicateCandidate CreateExpectedCandidate(
        Patient patient,
        User user,
        bool ssnLast4Matches = false
    ) =>
        new()
        {
            PatientId = patient.Id,
            FullName = patient.FullName,
            DateOfBirth = patient.DateOfBirth,
            SexAtBirth = patient.SexAtBirth,
            HomeAddress = patient.HomeAddress,
            Email = user.Email,
            Phone = user.Phone,
            SsnLast4Matches = ssnLast4Matches,
        };

    private static PatientDuplicateCandidate CreateExpectedCandidateWithoutContact(
        Patient patient,
        bool ssnLast4Matches = false
    ) =>
        new()
        {
            PatientId = patient.Id,
            FullName = patient.FullName,
            DateOfBirth = patient.DateOfBirth,
            SexAtBirth = patient.SexAtBirth,
            HomeAddress = patient.HomeAddress,
            SsnLast4Matches = ssnLast4Matches,
        };

    private async Task<Patient> CreatePatientAsync(string firstName, string firstSurname)
    {
        var patient = Patient.CreateProfile(
            PersonName.Create(firstName, null, firstSurname),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-67)),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetLocalNow().DateTime
        );
        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(
            EmergencyContact.Create("Contact", null, "Contact", "555-9999")
        );

        Context.Patients.Add(patient);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return patient;
    }

    private async Task CreatePatientsAsync(int count, string firstName, string firstSurname)
    {
        for (var i = 0; i < count; i++)
            await CreatePatientAsync(firstName, firstSurname);
    }

    private async Task<User> CreatePendingUserAsync()
    {
        var user = User.CreatePending(
            EmailAddress.Create($"{Guid.CreateVersion7()}@clinic.com"),
            PhoneNumber.Create($"+1555{Random.Shared.Next(1000000, 9999999)}")
        );

        Context.Users.Add(user);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return user;
    }

    private async Task<User> CreateAccountAsync(Guid patientId)
    {
        var user = await CreatePendingUserAsync();
        var membership = FamilyMembership.CreateSelf(
            patientId,
            user.Id,
            _fakeTime.GetUtcNow()
        );

        Context.FamilyMemberships.Add(membership);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return user;
    }

    private async Task<Patient> CreateSelfPatientAsync()
    {
        var patient = Patient.CreateProfile(
            PersonName.Create("fullName", null, "fullName"),
            new DateOnly(1990, 1, 1),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetLocalNow().DateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(
            EmergencyContact.Create("Contact", null, "Contact", "555-9999")
        );

        Context.Patients.Add(patient);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return patient;
    }

    private async Task<Patient> CreateHashedPatientAsync()
    {
        var patient = Patient.CreateProfile(
            PersonName.Create("Hashed", null, "Patient"),
            new DateOnly(1990, 1, 1),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetLocalNow().DateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(
            EmergencyContact.Create("Contact", null, "Contact", "555-9999")
        );

        patient.AssignSsnLast4Hash(
            "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
        );

        Context.Patients.Add(patient);

        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return patient;
    }
}
