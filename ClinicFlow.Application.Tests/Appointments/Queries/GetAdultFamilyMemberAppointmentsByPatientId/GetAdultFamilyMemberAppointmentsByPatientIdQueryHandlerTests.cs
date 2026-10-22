using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Appointments.Queries.GetAdultFamilyMemberAppointmentsByPatientId;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetAdultFamilyMemberAppointmentsByPatientId;

public class GetAdultFamilyMemberAppointmentsByPatientIdQueryHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly GetAdultFamilyMemberAppointmentsByPatientIdQueryHandler _sut;

    public GetAdultFamilyMemberAppointmentsByPatientIdQueryHandlerTests()
    {
        _sut = new GetAdultFamilyMemberAppointmentsByPatientIdQueryHandler(
            _familyMembershipRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _fakeTime
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredPaginatedList_WhenFamilyMemberHasRestrictedAccess()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetAdultFamilyMemberAppointmentsByPatientIdQuery(
            requesterUserId,
            patientId,
            1,
            10
        );

        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            requesterUserId,
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Restricted,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        membership.AddAllowedAppointmentCategory(
            AppointmentCategory.GeneralMedicine,
            requesterIsAuthorized: true
        );

        var patient = CreatePatient(30);

        var appointment = Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(9), new TimeOnly(10)),
            Guid.CreateVersion7()
        );

        var appointments = new List<Appointment> { appointment };

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetByPatientIdInCategoriesAsync(
                    patientId,
                    It.IsAny<IReadOnlyCollection<AppointmentCategory>>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((appointments, 1));

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDtos = appointments.Select(a => new PatientAppointmentDto(
            a.Id,
            a.PatientId,
            a.DoctorId,
            a.AppointmentTypeId,
            a.ScheduledDate,
            a.TimeSlot.Start,
            a.TimeSlot.End,
            a.Status,
            a.PatientNotes,
            a.ReceptionistNotes,
            a.GuardianNotes
        ));

        result.Items.Should().BeEquivalentTo(expectedDtos);
        result.TotalCount.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);

        _appointmentRepositoryMock.Verify(
            x =>
                x.GetByPatientIdInCategoriesAsync(
                    patientId,
                    It.IsAny<IReadOnlyCollection<AppointmentCategory>>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFilteredPaginatedList_WhenPatientIsExactlyAdultAge()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetAdultFamilyMemberAppointmentsByPatientIdQuery(
            requesterUserId,
            patientId,
            1,
            10
        );

        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            requesterUserId,
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Restricted,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        membership.AddAllowedAppointmentCategory(
            AppointmentCategory.GeneralMedicine,
            requesterIsAuthorized: true
        );

        var patient = CreatePatient(DomainRules.AdultAge);

        var appointment = Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(9), new TimeOnly(10)),
            Guid.CreateVersion7()
        );

        var appointments = new List<Appointment> { appointment };

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetByPatientIdInCategoriesAsync(
                    patientId,
                    It.IsAny<IReadOnlyCollection<AppointmentCategory>>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((appointments, 1));

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDtos = appointments.Select(a => new PatientAppointmentDto(
            a.Id,
            a.PatientId,
            a.DoctorId,
            a.AppointmentTypeId,
            a.ScheduledDate,
            a.TimeSlot.Start,
            a.TimeSlot.End,
            a.Status,
            a.PatientNotes,
            a.ReceptionistNotes,
            a.GuardianNotes
        ));

        result.Items.Should().BeEquivalentTo(expectedDtos);
        result.TotalCount.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);

        _appointmentRepositoryMock.Verify(
            x =>
                x.GetByPatientIdInCategoriesAsync(
                    patientId,
                    It.IsAny<IReadOnlyCollection<AppointmentCategory>>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainValidationException_WhenPatientIsMinor()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetAdultFamilyMemberAppointmentsByPatientIdQuery(
            requesterUserId,
            patientId,
            1,
            10
        );

        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            requesterUserId,
            PatientRelationship.Parent,
            LegalAuthorityType.Parent,
            FamilyMembershipAccessLevel.Full,
            15,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var patient = CreatePatient(DomainRules.AdultAge - 1);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        // Act
        var act = async () => await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.AdultHistoryRequiresAdult);

        _appointmentRepositoryMock.Verify(
            x =>
                x.GetByPatientIdInCategoriesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<AppointmentCategory>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenMembershipIsMissing()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetAdultFamilyMemberAppointmentsByPatientIdQuery(
            requesterUserId,
            patientId,
            1,
            10
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((FamilyMembership?)null);

        // Act
        var act = async () => await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(FamilyMembership));

        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientIsMissing()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetAdultFamilyMemberAppointmentsByPatientIdQuery(
            requesterUserId,
            patientId,
            1,
            10
        );

        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            requesterUserId,
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Restricted,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(membership);

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = async () => await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var exceptionAssertion = await act.Should()
            .ThrowAsync<EntityNotFoundException>()
            .WithMessage(DomainErrors.General.NotFound);
        exceptionAssertion.Which.EntityName.Should().Be(nameof(Patient));

        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _appointmentRepositoryMock.Verify(
            x =>
                x.GetByPatientIdInCategoriesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<AppointmentCategory>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    private Patient CreatePatient(int age)
    {
        var patient = Patient.CreateProfile(
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-age)),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        return patient;
    }
}
