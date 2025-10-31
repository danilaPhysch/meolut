using TleApi.Validators;

namespace TleApi.Tests;

public class TleValidatorTests
{
    [Theory]
    [InlineData("GPS", true)]
    [InlineData("GLONASS", true)]
    [InlineData("GALILEO", true)]
    [InlineData("BEIDOU", true)]
    [InlineData("gps", false)] // Case-sensitive
    [InlineData("INVALID", false)]
    [InlineData("", true)] // Empty is valid (optional)
    [InlineData(null, true)] // Null is valid (optional)
    public void IsValidSystem_WithVariousInputs_ReturnsExpectedResult(string? system, bool expected)
    {
        // Act
        var result = TleValidator.IsValidSystem(system, out var errorMessage);

        // Assert
        Assert.Equal(expected, result);
        if (expected)
        {
            Assert.Null(errorMessage);
        }
        else
        {
            Assert.NotNull(errorMessage);
        }
    }

    [Theory]
    [InlineData("G01", true)]
    [InlineData("R12", true)]
    [InlineData("E24", true)]
    [InlineData("C03", true)]
    [InlineData("1234567890", true)] // Max length
    [InlineData("12345678901", false)] // Too long
    [InlineData(" G01", false)] // Leading space
    [InlineData("G01 ", false)] // Trailing space
    [InlineData("", true)] // Empty is valid (optional)
    [InlineData(null, true)] // Null is valid (optional)
    public void IsValidPrn_WithVariousInputs_ReturnsExpectedResult(string? prn, bool expected)
    {
        // Act
        var result = TleValidator.IsValidPrn(prn, out var errorMessage);

        // Assert
        Assert.Equal(expected, result);
        if (expected)
        {
            Assert.Null(errorMessage);
        }
        else
        {
            Assert.NotNull(errorMessage);
        }
    }

    [Theory]
    [InlineData("2024-10-30T12:00:00Z", true)]
    [InlineData("2024-10-30T12:00:00.000Z", true)]
    [InlineData("2024-10-30", true)]
    [InlineData("", true)] // Empty is valid (optional)
    [InlineData(null, true)] // Null is valid (optional)
    [InlineData("invalid", false)]
    [InlineData("2024-13-01", false)] // Invalid month
    public void IsValidDateTime_WithVariousInputs_ReturnsExpectedResult(string? datetime, bool expected)
    {
        // Act
        var result = TleValidator.IsValidDateTime(datetime, out var parsedDateTime, out var errorMessage);

        // Assert
        Assert.Equal(expected, result);
        if (expected)
        {
            Assert.Null(errorMessage);
            if (!string.IsNullOrWhiteSpace(datetime))
            {
                Assert.NotNull(parsedDateTime);
            }
        }
        else
        {
            Assert.NotNull(errorMessage);
        }
    }

    [Theory]
    [InlineData(1, 50, true)]
    [InlineData(1, 1, true)]
    [InlineData(1, 200, true)]
    [InlineData(0, 50, false)] // Page too small
    [InlineData(1, 0, false)] // PageSize too small
    [InlineData(1, 201, false)] // PageSize too large
    [InlineData(-1, 50, false)] // Negative page
    public void IsValidPagination_WithVariousInputs_ReturnsExpectedResult(int page, int pageSize, bool expected)
    {
        // Act
        var result = TleValidator.IsValidPagination(page, pageSize, out var errorMessage);

        // Assert
        Assert.Equal(expected, result);
        if (expected)
        {
            Assert.Null(errorMessage);
        }
        else
        {
            Assert.NotNull(errorMessage);
        }
    }
}
