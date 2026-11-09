using SchoolAdmission.Application.Classes.Commands.CreateClass;
using SchoolAdmission.Application.Holds.Commands.CreateHold;

namespace SchoolAdmission.Application.UnitTests.Validators;

public sealed class RequestValidatorTests
{
    private readonly CreateClassCommandValidator _classValidator = new();
    private readonly CreateHoldCommandValidator _holdValidator = new();

    [Theory]
    [InlineData("", 10)]
    [InlineData("   ", 10)]
    [InlineData("Class 6", 0)]
    [InlineData("Class 6", -1)]
    [InlineData("Class 6", 10_001)]
    public void Create_class_rejects_invalid_input(string name, int seats)
    {
        var result = _classValidator.Validate(new CreateClassCommand(name, seats));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10_000)]
    public void Create_class_accepts_seat_boundaries(int seats)
    {
        var result = _classValidator.Validate(new CreateClassCommand("Class 6", seats));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_class_accepts_a_two_hundred_character_name()
    {
        var result = _classValidator.Validate(
            new CreateClassCommand(new string('a', 200), 10));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_class_rejects_a_two_hundred_and_one_character_name()
    {
        var result = _classValidator.Validate(
            new CreateClassCommand(new string('a', 201), 10));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Create_class_accepts_valid_input()
    {
        var result = _classValidator.Validate(
            new CreateClassCommand("Class 6 - Morning Shift", 120));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_hold_rejects_an_empty_class_id()
    {
        var result = _holdValidator.Validate(
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
        var result = _holdValidator.Validate(new CreateHoldCommand("c_1", studentId));

        Assert.Equal(expected, result.IsValid);
    }
}
