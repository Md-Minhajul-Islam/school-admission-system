using SchoolAdmission.Application.Classes.Commands.CreateClass;

namespace SchoolAdmission.Application.UnitTests.Application.Classes.Commands.CreateClass;

public sealed class CreateClassCommandValidatorTests
{
    private readonly CreateClassCommandValidator _validator = new();

    [Theory]
    [InlineData("", 10)]
    [InlineData("   ", 10)]
    [InlineData("Class 6", 0)]
    [InlineData("Class 6", -1)]
    [InlineData("Class 6", 10_001)]
    public void Create_class_rejects_invalid_input(string name, int seats)
    {
        var result = _validator.Validate(new CreateClassCommand(name, seats));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10_000)]
    public void Create_class_accepts_seat_boundaries(int seats)
    {
        var result = _validator.Validate(new CreateClassCommand("Class 6", seats));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_class_accepts_a_two_hundred_character_name()
    {
        var result = _validator.Validate(
            new CreateClassCommand(new string('a', 200), 10));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_class_rejects_a_two_hundred_and_one_character_name()
    {
        var result = _validator.Validate(
            new CreateClassCommand(new string('a', 201), 10));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Create_class_accepts_valid_input()
    {
        var result = _validator.Validate(
            new CreateClassCommand("Class 6 - Morning Shift", 120));

        Assert.True(result.IsValid);
    }
}
