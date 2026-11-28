using AwesomeAssertions;
using ClinicFlow.Application.Interfaces;
using ClinicFlow.Application.Patients.Queries.DTOs;
using ClinicFlow.Application.Patients.Queries.PreCheckMinorPatientDuplicates;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services.Contexts.Registration;
using ClinicFlow.Domain.ValueObjects;
using Moq;

namespace ClinicFlow.Application.Tests.Patients.Queries.PreCheckMinorPatientDuplicates;

public class PreCheckMinorPatientDuplicatesQueryHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<ISsnHasher> _ssnHasherMock = new();
    private readonly PreCheckMinorPatientDuplicatesQueryHandler _sut;

    public PreCheckMinorPatientDuplicatesQueryHandlerTests()
    {
        _sut = new PreCheckMinorPatientDuplicatesQueryHandler(
            _patientRepositoryMock.Object,
            _ssnHasherMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnScoredAndMaskedCandidates_WhenCandidatesExist()
    {
        // Arrange
        var query = CreateQuery(ssnLast4: null);
        var fullName = PersonName.Create(query.FirstName, null, query.LastName);

        var candidate1 = CreateCandidate(
            firstName: query.FirstName,
            lastName: query.LastName,
            dateOfBirth: query.DateOfBirth,
            sex: query.SexAtBirth,
            addressLine: query.AddressLine,
            addressState: query.AddressState,
            postalCode: query.AddressPostalCode,
            country: query.AddressCountry,
            email: EmailAddress.Create("fluttersy@gmail.com"),
            phone: PhoneNumber.Create("+52 1 55 1234 5678"),
            ssnLast4Hash: "hash-1"
        );

        var candidate2 = CreateCandidate(
            firstName: query.FirstName,
            lastName: query.LastName,
            dateOfBirth: new DateOnly(2005, 1, 1),
            sex: BiologicalSex.Female,
            addressLine: "Ponyville",
            addressState: AddressState.Florida,
            postalCode: "33101",
            country: Country.UnitedStates,
            ssnLast4Hash: null
        );

        _patientRepositoryMock
            .Setup(x =>
                x.GetFuzzyMatchesBySearchNameAsync(
                    fullName.SearchName,
                    query.DateOfBirth,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([candidate1, candidate2]);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedScore1 = new PatientDuplicatePreCheckScoreDto(
            candidate1.PatientId,
            candidate1.FullName.ToString(),
            candidate1.DateOfBirth,
            candidate1.SexAtBirth,
            candidate1.HomeAddress.Line,
            candidate1.HomeAddress.State,
            candidate1.HomeAddress.PostalCode,
            "flu***@gmail.com",
            "****5678",
            70
        );

        var expectedScore2 = new PatientDuplicatePreCheckScoreDto(
            candidate2.PatientId,
            candidate2.FullName.ToString(),
            candidate2.DateOfBirth,
            candidate2.SexAtBirth,
            candidate2.HomeAddress.Line,
            candidate2.HomeAddress.State,
            candidate2.HomeAddress.PostalCode,
            null,
            null,
            35
        );

        var expectedDto = new PatientDuplicatePreCheckDto(
            [expectedScore1, expectedScore2],
            MaxScore: 70,
            IsHighBand: true
        );

        result.Should().BeEquivalentTo(expectedDto);

        _patientRepositoryMock.Verify(
            x =>
                x.GetFuzzyMatchesBySearchNameAsync(
                    fullName.SearchName,
                    query.DateOfBirth,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _ssnHasherMock.Verify(
            x => x.ComputeHash(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldMarkSsnMatchesAndIncludeWeight_WhenSsnLast4Matches()
    {
        // Arrange
        var query = CreateQuery(ssnLast4: "1234");
        var fullName = PersonName.Create(query.FirstName, null, query.LastName);

        var candidate = CreateCandidate(
            firstName: query.FirstName,
            lastName: query.LastName,
            dateOfBirth: query.DateOfBirth,
            sex: query.SexAtBirth,
            addressLine: query.AddressLine,
            addressState: query.AddressState,
            postalCode: query.AddressPostalCode,
            country: query.AddressCountry,
            ssnLast4Hash: "computed-hash-1234"
        );

        _patientRepositoryMock
            .Setup(x =>
                x.GetFuzzyMatchesBySearchNameAsync(
                    fullName.SearchName,
                    query.DateOfBirth,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([candidate]);

        _ssnHasherMock
            .Setup(x => x.ComputeHash(candidate.PatientId, "1234"))
            .Returns("computed-hash-1234");

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedScore = new PatientDuplicatePreCheckScoreDto(
            candidate.PatientId,
            candidate.FullName.ToString(),
            candidate.DateOfBirth,
            candidate.SexAtBirth,
            candidate.HomeAddress.Line,
            candidate.HomeAddress.State,
            candidate.HomeAddress.PostalCode,
            null,
            null,
            90
        );

        var expectedDto = new PatientDuplicatePreCheckDto(
            [expectedScore],
            MaxScore: 90,
            IsHighBand: true
        );

        result.Should().BeEquivalentTo(expectedDto);

        _patientRepositoryMock.Verify(
            x =>
                x.GetFuzzyMatchesBySearchNameAsync(
                    fullName.SearchName,
                    query.DateOfBirth,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        _ssnHasherMock.Verify(x => x.ComputeHash(candidate.PatientId, "1234"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotMarkSsnMatches_WhenSsnLast4DoesNotMatch()
    {
        // Arrange
        var query = CreateQuery(ssnLast4: "1234");
        var fullName = PersonName.Create(query.FirstName, null, query.LastName);

        var candidate = CreateCandidate(
            firstName: query.FirstName,
            lastName: query.LastName,
            dateOfBirth: query.DateOfBirth,
            sex: query.SexAtBirth,
            addressLine: query.AddressLine,
            addressState: query.AddressState,
            postalCode: query.AddressPostalCode,
            country: query.AddressCountry,
            ssnLast4Hash: "different-stored-hash"
        );

        _patientRepositoryMock
            .Setup(x =>
                x.GetFuzzyMatchesBySearchNameAsync(
                    fullName.SearchName,
                    query.DateOfBirth,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([candidate]);

        _ssnHasherMock
            .Setup(x => x.ComputeHash(candidate.PatientId, "1234"))
            .Returns("computed-hash-1234");

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedScore = new PatientDuplicatePreCheckScoreDto(
            candidate.PatientId,
            candidate.FullName.ToString(),
            candidate.DateOfBirth,
            candidate.SexAtBirth,
            candidate.HomeAddress.Line,
            candidate.HomeAddress.State,
            candidate.HomeAddress.PostalCode,
            null,
            null,
            70
        );

        var expectedDto = new PatientDuplicatePreCheckDto(
            [expectedScore],
            MaxScore: 70,
            IsHighBand: true
        );

        result.Should().BeEquivalentTo(expectedDto);

        _patientRepositoryMock.Verify(
            x =>
                x.GetFuzzyMatchesBySearchNameAsync(
                    fullName.SearchName,
                    query.DateOfBirth,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        _ssnHasherMock.Verify(x => x.ComputeHash(candidate.PatientId, "1234"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotMarkSsnMatches_WhenSsnLast4IsNull()
    {
        // Arrange
        var query = CreateQuery(ssnLast4: null);
        var fullName = PersonName.Create(query.FirstName, null, query.LastName);

        var candidate = CreateCandidate(
            firstName: query.FirstName,
            lastName: query.LastName,
            dateOfBirth: query.DateOfBirth,
            sex: query.SexAtBirth,
            addressLine: query.AddressLine,
            addressState: query.AddressState,
            postalCode: query.AddressPostalCode,
            country: query.AddressCountry,
            ssnLast4Hash: "stored-hash"
        );

        _patientRepositoryMock
            .Setup(x =>
                x.GetFuzzyMatchesBySearchNameAsync(
                    fullName.SearchName,
                    query.DateOfBirth,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([candidate]);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedScore = new PatientDuplicatePreCheckScoreDto(
            candidate.PatientId,
            candidate.FullName.ToString(),
            candidate.DateOfBirth,
            candidate.SexAtBirth,
            candidate.HomeAddress.Line,
            candidate.HomeAddress.State,
            candidate.HomeAddress.PostalCode,
            null,
            null,
            70
        );

        var expectedDto = new PatientDuplicatePreCheckDto(
            [expectedScore],
            MaxScore: 70,
            IsHighBand: true
        );

        result.Should().BeEquivalentTo(expectedDto);

        _ssnHasherMock.Verify(
            x => x.ComputeHash(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoCandidatesFound()
    {
        // Arrange
        var query = CreateQuery();
        var fullName = PersonName.Create(query.FirstName, null, query.LastName);

        _patientRepositoryMock
            .Setup(x =>
                x.GetFuzzyMatchesBySearchNameAsync(
                    fullName.SearchName,
                    query.DateOfBirth,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([]);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDto = new PatientDuplicatePreCheckDto([], MaxScore: 0, IsHighBand: false);

        result.Should().BeEquivalentTo(expectedDto);

        _patientRepositoryMock.Verify(
            x =>
                x.GetFuzzyMatchesBySearchNameAsync(
                    fullName.SearchName,
                    query.DateOfBirth,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _ssnHasherMock.Verify(
            x => x.ComputeHash(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldScoreThroughMinorWeights_WhenCandidateCarriesAccountContactData()
    {
        // Arrange
        var query = CreateQuery(ssnLast4: null);
        var fullName = PersonName.Create(query.FirstName, null, query.LastName);

        var candidate = CreateCandidate(
            firstName: query.FirstName,
            lastName: query.LastName,
            dateOfBirth: query.DateOfBirth,
            sex: query.SexAtBirth,
            addressLine: query.AddressLine,
            addressState: query.AddressState,
            postalCode: query.AddressPostalCode,
            country: query.AddressCountry,
            email: EmailAddress.Create("rainbow.dashy@clinicflow.test"),
            phone: PhoneNumber.Create("+1 555 1234567")
        );

        _patientRepositoryMock
            .Setup(x =>
                x.GetFuzzyMatchesBySearchNameAsync(
                    fullName.SearchName,
                    query.DateOfBirth,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([candidate]);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedScore = new PatientDuplicatePreCheckScoreDto(
            candidate.PatientId,
            candidate.FullName.ToString(),
            candidate.DateOfBirth,
            candidate.SexAtBirth,
            candidate.HomeAddress.Line,
            candidate.HomeAddress.State,
            candidate.HomeAddress.PostalCode,
            "rai***@clinicflow.test",
            "****4567",
            70
        );

        var expectedDto = new PatientDuplicatePreCheckDto(
            [expectedScore],
            MaxScore: 70,
            IsHighBand: true
        );

        result.Should().BeEquivalentTo(expectedDto);

        _patientRepositoryMock.Verify(
            x =>
                x.GetFuzzyMatchesBySearchNameAsync(
                    fullName.SearchName,
                    query.DateOfBirth,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _ssnHasherMock.Verify(
            x => x.ComputeHash(It.IsAny<Guid>(), It.IsAny<string>()),
            Times.Never
        );
    }

    private static PreCheckMinorPatientDuplicatesQuery CreateQuery(string? ssnLast4 = null) =>
        new(
            Guid.CreateVersion7(),
            "Fluttershy",
            "Tests",
            new DateOnly(2010, 5, 15),
            BiologicalSex.Female,
            "Ponyville",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            ssnLast4
        );

    private static PatientDuplicateCandidate CreateCandidate(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        BiologicalSex sex,
        string addressLine,
        AddressState addressState,
        string postalCode,
        Country country,
        EmailAddress? email = null,
        PhoneNumber? phone = null,
        string? ssnLast4Hash = null
    ) =>
        new()
        {
            PatientId = Guid.CreateVersion7(),
            FullName = PersonName.Create(firstName, null, lastName),
            DateOfBirth = dateOfBirth,
            SexAtBirth = sex,
            HomeAddress = Address.Create(addressLine, addressState, postalCode, country),
            Email = email,
            Phone = phone,
            SsnLast4Hash = ssnLast4Hash,
        };
}
