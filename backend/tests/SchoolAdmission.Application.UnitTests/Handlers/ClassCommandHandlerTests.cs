using AutoMapper;
using NSubstitute;
using SchoolAdmission.Application.Classes.Commands.CreateClass;
using SchoolAdmission.Application.Common.Abstractions.Persistence;
using SchoolAdmission.Domain.Entities;

namespace SchoolAdmission.Application.UnitTests.Handlers;

public sealed class ClassCommandHandlerTests
{
    private readonly IClassRoomRepository _classes = Substitute.For<IClassRoomRepository>();
    private readonly IRepository _repository = Substitute.For<IRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();

    [Fact]
    public async Task Create_trims_the_class_name_before_constructing_the_entity()
    {
        ClassRoom? addedClass = null;
        _classes.AddAsync(
                Arg.Do<ClassRoom>(classRoom => addedClass = classRoom),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        _mapper.Map<ClassCreatedDto>(Arg.Any<ClassRoom>())
            .Returns(new ClassCreatedDto("c_1", "Class 6", 10));

        var handler = new CreateClassCommandHandler(
            _classes,
            _repository,
            _mapper,
            TimeProvider.System);

        var result = await handler.Handle(
            new CreateClassCommand("  Class 6  ", 10),
            CancellationToken.None);

        var classRoom = Assert.IsType<ClassRoom>(addedClass);
        Assert.Equal("Class 6", classRoom.Name);
        Assert.Equal(10, classRoom.TotalSeats);
        Assert.Equal("Class 6", result.Name);
    }
}
