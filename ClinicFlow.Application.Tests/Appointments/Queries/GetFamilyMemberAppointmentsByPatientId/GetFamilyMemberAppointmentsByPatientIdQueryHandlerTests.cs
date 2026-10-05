using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Appointments.Queries.GetFamilyMemberAppointmentsByPatientId;
using ClinicFlow.Application.Tests.Shared;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.Services.Policies;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetFamilyMemberAppointmentsByPatientId;

public class GetFamilyMemberAppointmentsByPatientIdQueryHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly GetFamilyMemberAppointmentsByPatientIdQueryHandler _sut;

    public GetFamilyMemberAppointmentsByPatientIdQueryHandlerTests()
    {
        _sut = new GetFamilyMemberAppointmentsByPatientIdQueryHandler(
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
        var query = new GetFamilyMemberAppointmentsByPatientIdQuery(
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

        var patient = CreatePatient(patientId, 30);

        var appointment = Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(9), new TimeOnly(10)),
            Guid.CreateVersion7()
        );

        var appointments = new List<Appointment> { appointment };
        var allowedCaptured = new List<IReadOnlyCollection<AppointmentCategory>>();
        var excludedCaptured = new List<IReadOnlyCollection<ProtectedCategory>>();

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
                x.GetByPatientIdInCategoriesExcludingProtectedAsync(
                    patientId,
                    Capture.In(allowedCaptured),
                    Capture.In(excludedCaptured),
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
            a.TimeRange.Start,
            a.TimeRange.End,
            a.Status,
            a.PatientNotes,
            a.ReceptionistNotes,
            a.GuardianNotes
        ));

        result.Items.Should().BeEquivalentTo(expectedDtos);
        result.TotalCount.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);

        allowedCaptured.Single().Should().BeEquivalentTo([AppointmentCategory.GeneralMedicine]);
        excludedCaptured
            .Single()
            .Should()
            .BeEquivalentTo(ProtectedCategoryPolicy.GetProtectedCategoriesFor(30));

        _familyMembershipRepositoryMock.Verify(
            x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _appointmentRepositoryMock.Verify(
            x =>
                x.GetByPatientIdInCategoriesExcludingProtectedAsync(
                    patientId,
                    It.IsAny<IReadOnlyCollection<AppointmentCategory>>(),
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenNoVisibleAppointments()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetFamilyMemberAppointmentsByPatientIdQuery(
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

        var patient = CreatePatient(patientId, 30);

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
                x.GetByPatientIdInCategoriesExcludingProtectedAsync(
                    patientId,
                    It.IsAny<IReadOnlyCollection<AppointmentCategory>>(),
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    1,
                    10,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((new List<Appointment>(), 0));

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenMembershipIsMissing()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetFamilyMemberAppointmentsByPatientIdQuery(
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

        _familyMembershipRepositoryMock.Verify(
            x =>
                x.GetActiveMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        _patientRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        _appointmentRepositoryMock.Verify(
            x =>
                x.GetByPatientIdInCategoriesExcludingProtectedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<AppointmentCategory>>(),
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldThrowEntityNotFoundException_WhenPatientNotFound()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetFamilyMemberAppointmentsByPatientIdQuery(
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

        _appointmentRepositoryMock.Verify(
            x =>
                x.GetByPatientIdInCategoriesExcludingProtectedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<AppointmentCategory>>(),
                    It.IsAny<IReadOnlyCollection<ProtectedCategory>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    private Patient CreatePatient(Guid id, int age)
    {
        var patient = Patient.CreateProfile(
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-age)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.SetId(id);

        return patient;
    }
}
