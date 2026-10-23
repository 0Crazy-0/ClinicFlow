using AwesomeAssertions;
using ClinicFlow.Application.Interfaces;
using ClinicFlow.Application.Patients.Queries.CheckPatientSsnLast4;
using ClinicFlow.Domain.Interfaces.Repositories;
using Moq;

namespace ClinicFlow.Application.Tests.Patients.Queries.CheckPatientSsnLast4;

public class CheckPatientSsnLast4QueryHandlerTests
{
    private readonly Mock<ISsnHasher> _ssnHasherMock;
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly CheckPatientSsnLast4QueryHandler _sut;

    public CheckPatientSsnLast4QueryHandlerTests()
    {
        _ssnHasherMock = new Mock<ISsnHasher>();
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _sut = new CheckPatientSsnLast4QueryHandler(
            _ssnHasherMock.Object,
            _patientRepositoryMock.Object
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_ShouldReturnRepositoryResult_WhenHashIsComputed(bool expectedResult)
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var last4 = "1234";
        var ssnLast4Hash = "hashed-ssn";

        _ssnHasherMock.Setup(x => x.ComputeHash(patientId, last4)).Returns(ssnLast4Hash);

        _patientRepositoryMock
            .Setup(x =>
                x.ExistsWithSsnLast4HashAsync(
                    patientId,
                    ssnLast4Hash,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedResult);

        var query = new CheckPatientSsnLast4Query(patientId, last4);

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(expectedResult);

        _ssnHasherMock.Verify(x => x.ComputeHash(patientId, last4), Times.Once);

        _patientRepositoryMock.Verify(
            x =>
                x.ExistsWithSsnLast4HashAsync(
                    patientId,
                    ssnLast4Hash,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
