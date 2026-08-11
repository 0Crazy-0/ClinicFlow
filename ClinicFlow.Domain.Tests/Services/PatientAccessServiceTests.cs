using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Services;

namespace ClinicFlow.Domain.Tests.Services;

public class PatientAccessServiceTests
{
    [Fact]
    public void VerifyAccess_ShouldThrowPatientAccessUnauthorizedException_WhenInitiatorHasAccessToTargetIsFalse()
    {
        // Act
        var act = () => PatientAccessService.VerifyAccess(false);

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void VerifyAccess_ShouldNotThrow_WhenInitiatorHasAccessToTargetIsTrue()
    {
        // Act
        var act = () => PatientAccessService.VerifyAccess(true);

        // Assert
        act.Should().NotThrow();
    }
}
