using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Appointments.Queries.GetMinorFamilyMemberAppointmentsByPatientId;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetMinorFamilyMemberAppointmentsByPatientId;

public class GetMinorFamilyMemberAppointmentsByPatientIdQueryHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IAppointmentConsentGrantRepository> _consentGrantRepositoryMock = new();
    private readonly GetMinorFamilyMemberAppointmentsByPatientIdQueryHandler _sut;

    public GetMinorFamilyMemberAppointmentsByPatientIdQueryHandlerTests()
    {
        _sut = new GetMinorFamilyMemberAppointmentsByPatientIdQueryHandler(
            _familyMembershipRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _consentGrantRepositoryMock.Object,
            _fakeTime
        );
    }

    [Fact]
    public async Task Handle_ShouldHidePatientNotes_WhenVisibleThroughSchedulerException()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetMinorFamilyMemberAppointmentsByPatientIdQuery(
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

        var appointment = Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(9), new TimeOnly(10)),
            requesterUserId
        );

        appointment.UpdatePatientNotes("notes");

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

        _consentGrantRepositoryMock
            .Setup(x =>
                x.GetEffectiveCategoriesAsync(
                    patientId,
                    membership.Id,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([]);

        _consentGrantRepositoryMock
            .Setup(x =>
                x.GetEffectiveAppointmentIdsAsync(
                    patientId,
                    membership.Id,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([]);

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetMinorVisibleAppointmentsAsync(
                    patientId,
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    requesterUserId,
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<Appointment> { appointment }, 1));

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDto = new MinorFamilyAppointmentDto(
            appointment.Id,
            appointment.PatientId,
            appointment.DoctorId,
            appointment.AppointmentTypeId,
            appointment.ScheduledDate,
            appointment.TimeSlot.Start,
            appointment.TimeSlot.End,
            appointment.Status,
            appointment.ReceptionistNotes,
            appointment.GuardianNotes
        );

        result.Items.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task Handle_ShouldHidePatientNotes_WhenCategoryGrantCoversIt()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetMinorFamilyMemberAppointmentsByPatientIdQuery(
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

        var appointment = Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(9), new TimeOnly(10)),
            requesterUserId
        );

        appointment.UpdatePatientNotes("notes");

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

        _consentGrantRepositoryMock
            .Setup(x =>
                x.GetEffectiveCategoriesAsync(
                    patientId,
                    membership.Id,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([ProtectedCategory.MentalHealthCounseling]);

        _consentGrantRepositoryMock
            .Setup(x =>
                x.GetEffectiveAppointmentIdsAsync(
                    patientId,
                    membership.Id,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([]);

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetMinorVisibleAppointmentsAsync(
                    patientId,
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    requesterUserId,
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<Appointment> { appointment }, 1));

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        var expectedDto = new MinorFamilyAppointmentDto(
            appointment.Id,
            appointment.PatientId,
            appointment.DoctorId,
            appointment.AppointmentTypeId,
            appointment.ScheduledDate,
            appointment.TimeSlot.Start,
            appointment.TimeSlot.End,
            appointment.Status,
            appointment.ReceptionistNotes,
            appointment.GuardianNotes
        );

        result.Items.Should().ContainSingle().Which.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task Handle_ShouldThrowPatientAccessUnauthorizedException_WhenRequesterHasNoLegalAuthority()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetMinorFamilyMemberAppointmentsByPatientIdQuery(
            requesterUserId,
            patientId,
            1,
            10
        );
        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            requesterUserId,
            PatientRelationship.Parent,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var patient = CreatePatient(15);

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
            .ThrowAsync<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);

        _appointmentRepositoryMock.Verify(
            x =>
                x.GetMinorVisibleAppointmentsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainValidationException_WhenPatientIsAdult()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetMinorFamilyMemberAppointmentsByPatientIdQuery(
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
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var patient = CreatePatient(30);

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
            .WithMessage(DomainErrors.Appointment.MinorHistoryRequiresMinor);

        _consentGrantRepositoryMock.Verify(
            x =>
                x.GetEffectiveCategoriesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainValidationException_WhenPatientIsExactlyAdultAge()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetMinorFamilyMemberAppointmentsByPatientIdQuery(
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
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var patient = CreatePatient(DomainRules.AdultAge);

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
            .WithMessage(DomainErrors.Appointment.MinorHistoryRequiresMinor);

        _consentGrantRepositoryMock.Verify(
            x =>
                x.GetEffectiveCategoriesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DateTime>(),
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
        var query = new GetMinorFamilyMemberAppointmentsByPatientIdQuery(
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
        var query = new GetMinorFamilyMemberAppointmentsByPatientIdQuery(
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

        _consentGrantRepositoryMock.Verify(
            x =>
                x.GetEffectiveCategoriesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );

        _appointmentRepositoryMock.Verify(
            x =>
                x.GetMinorVisibleAppointmentsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<Guid>(),
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
            _fakeTime.GetUtcNow().UtcDateTime
        );

        return patient;
    }
}
