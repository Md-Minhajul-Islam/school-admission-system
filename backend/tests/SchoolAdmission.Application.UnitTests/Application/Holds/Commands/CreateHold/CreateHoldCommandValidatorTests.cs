using SchoolAdmission.Application.Holds.Commands.CreateHold;

namespace SchoolAdmission.Application.UnitTests.Application.Holds.Commands.CreateHold;

public sealed class CreateHoldCommandValidatorTests
{
    private readonly CreateHoldCommandValidator _validator = new();

    [Fact]
    public void Create_hold_rejects_an_empty_class_id()
    {
        var result = _validator.Validate(
            new CreateHoldCommand("", "20152691234567890"));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("20152691234567890", true)]
    [InlineData("2015269123456789", false)]
    [InlineData("201526912345678901", false)]
    [InlineData("2015269123456789X", false)]
    [InlineData("", false)]
    public void Create_hold_requires_a_seventeen_digit_student_id(string studentId, bool expected)
    {
        var result = _validator.Validate(new CreateHoldCommand("c_1", studentId));

        Assert.Equal(expected, result.IsValid);
    }
}
