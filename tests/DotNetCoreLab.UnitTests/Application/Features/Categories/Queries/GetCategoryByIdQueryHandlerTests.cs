using DotNetCoreLab.Application.Features.Categories.Queries.GetCategoryById;
using DotNetCoreLab.Application.Features.Categories.Dtos;
using DotNetCoreLab.Domain.Entities;
using DotNetCoreLab.Domain.Interfaces;
using Moq;
using AutoMapper;

namespace DotNetCoreLab.UnitTests.Application.Features.Categories.Queries;

public class GetCategoryByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_CheckCategoryAndGetCategoryDto()
    {
        var testId = Guid.NewGuid();
        var fakeCategory = new Category { Id = testId, Name = "Elektronik", IsActive = true };
        var fakeDto = new CategoryDto { Id = testId, Name = "Elektronik" };

        var mockRepository = new Mock<ICategoryRepository>();
        mockRepository.Setup(repo => repo.GetCategoryByIdAsync(testId)).ReturnsAsync(fakeCategory);

        var mockMapper = new Mock<IMapper>();
        mockMapper.Setup(m => m.Map<CategoryDto>(It.IsAny<Category>())).Returns(fakeDto);

        var handler = new GetCategoryByIdQueryHandler(mockRepository.Object, mockMapper.Object);
        var query = new GetCategoryByIdQuery(testId);
        var cancellationToken = new CancellationToken();

        var result = await handler.Handle(query, cancellationToken);

        Assert.NotNull(result);
        Assert.Equal("Elektronik", result.Name);
    }

    [Fact]
    public async Task Handle_NotFindCategoryAndGetNull()
    {
        var testId = Guid.NewGuid();
        var mockRepository = new Mock<ICategoryRepository>();
        Category? nullCategory = null;

        mockRepository.Setup(x => x.GetCategoryByIdAsync(testId)).ReturnsAsync(nullCategory);

        var mockMapper = new Mock<IMapper>();

        var handler = new GetCategoryByIdQueryHandler(mockRepository.Object, mockMapper.Object);
        var query = new GetCategoryByIdQuery(testId);
        CancellationToken token = new CancellationToken();

        var result = await handler.Handle(query, token);

        Assert.Null(result);
    }
}