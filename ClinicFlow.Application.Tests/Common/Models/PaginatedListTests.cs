using AwesomeAssertions;
using ClinicFlow.Application.Common.Models;

namespace ClinicFlow.Application.Tests.Common.Models;

public class PaginatedListTests
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(5, 10, 1)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(20, 10, 2)]
    [InlineData(25, 10, 3)]
    public void TotalPages_ShouldCalculateExpectedTotalPages(
        int totalCount,
        int pageSize,
        int expectedTotalPages
    )
    {
        // Arrange & Act
        var sut = new PaginatedList<string>([], totalCount, pageNumber: 1, pageSize: pageSize);

        // Assert
        sut.TotalPages.Should().Be(expectedTotalPages);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(5, true)]
    public void HasPreviousPage_ShouldReturnExpectedValue(
        int pageNumber,
        bool expectedHasPreviousPage
    )
    {
        // Arrange & Act
        var sut = new PaginatedList<string>([], totalCount: 50, pageNumber: pageNumber, pageSize: 10);

        // Assert
        sut.HasPreviousPage.Should().Be(expectedHasPreviousPage);
    }

    [Theory]
    [InlineData(1, 30, 10, true)]
    [InlineData(2, 30, 10, true)]
    [InlineData(3, 30, 10, false)]
    [InlineData(4, 30, 10, false)]
    [InlineData(1, 0, 10, false)]
    public void HasNextPage_ShouldReturnExpectedValue(
        int pageNumber,
        int totalCount,
        int pageSize,
        bool expectedHasNextPage
    )
    {
        // Arrange & Act
        var sut = new PaginatedList<string>([], totalCount, pageNumber, pageSize);

        // Assert
        sut.HasNextPage.Should().Be(expectedHasNextPage);
    }
}
