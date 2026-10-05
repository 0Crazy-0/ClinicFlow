using AwesomeAssertions;
using ClinicFlow.Application.Appointments.Queries.DTOs;
using ClinicFlow.Application.Appointments.Queries.GetOwnAppointmentsByPatientId;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace ClinicFlow.Application.Tests.Appointments.Queries.GetOwnAppointmentsByPatientId;

public class GetOwnAppointmentsByPatientIdQueryHandlerTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly Mock<IFamilyMembershipRepository> _familyMembershipRepositoryMock = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly GetOwnAppointmentsByPatientIdQueryHandler _sut;

    public GetOwnAppointmentsByPatientIdQueryHandlerTests()
    {
        _sut = new GetOwnAppointmentsByPatientIdQueryHandler(
            _familyMembershipRepositoryMock.Object,
            _appointmentRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WhenRequesterIsSelf()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetOwnAppointmentsByPatientIdQuery(requesterUserId, patientId, 1, 10);
        var appointments = new List<Appointment>
        {
            CreateAppointment(patientId),
            CreateAppointment(patientId),
        };

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetByPatientIdPaginatedAsync(patientId, 1, 10, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync((appointments, 2));

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
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(1);

        _familyMembershipRepositoryMock.Verify(
            x =>
                x.HasActiveSelfMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _appointmentRepositoryMock.Verify(
            x => x.GetByPatientIdPaginatedAsync(patientId, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPaginatedList_WhenPatientHasNoAppointments()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetOwnAppointmentsByPatientIdQuery(requesterUserId, patientId, 1, 10);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        _appointmentRepositoryMock
            .Setup(x =>
                x.GetByPatientIdPaginatedAsync(patientId, 1, 10, It.IsAny<CancellationToken>())
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
    public async Task Handle_ShouldThrowDomainValidationException_WhenRequesterIsNotSelf()
    {
        // Arrange
        var requesterUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var query = new GetOwnAppointmentsByPatientIdQuery(requesterUserId, patientId, 1, 10);

        _familyMembershipRepositoryMock
            .Setup(x =>
                x.HasActiveSelfMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        // Act
        var act = async () => await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedAccess);

        _familyMembershipRepositoryMock.Verify(
            x =>
                x.HasActiveSelfMembershipAsync(
                    requesterUserId,
                    patientId,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _appointmentRepositoryMock.Verify(
            x =>
                x.GetByPatientIdPaginatedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    private Appointment CreateAppointment(Guid patientId) =>
        Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            Guid.CreateVersion7()
        );
}
