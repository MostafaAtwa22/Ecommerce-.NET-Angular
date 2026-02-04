using AutoMapper;
using Ecommerce.API.Dtos;
using Ecommerce.API.Errors;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Spec;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Helpers;

public static class PaginationExtensions
{
    public static async Task<ActionResult<Pagination<TDestination>>> ToPagedResultAsync<TEntity, TDestination>(
        this ControllerBase controller,
        IUnitOfWork unitOfWork,
        ISpecifications<TEntity> dataSpec,
        ISpecifications<TEntity> countSpec,
        int pageIndex,
        int pageSize,
        IMapper mapper)
        where TEntity : BaseEntity
        where TDestination : class
    {
        var repository = unitOfWork.Repository<TEntity>();

        var totalItems = await repository.CountAsync(countSpec);
        var entities = await repository.GetAllWithSpecAsync(dataSpec);

        var data = mapper.Map<IReadOnlyList<TDestination>>(entities);

        return controller.Ok(new Pagination<TDestination>(pageIndex, pageSize, totalItems, data));
    }

    public static ActionResult NotFoundResponse(this ControllerBase controller)
        => controller.NotFound(new ApiResponse(StatusCodes.Status404NotFound));
}

