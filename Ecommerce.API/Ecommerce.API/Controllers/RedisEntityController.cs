namespace Ecommerce.API.Controllers
{
    public abstract class RedisEntityController<TDto, TEntity> : BaseApiController
        where TDto : IRedisDto
        where TEntity : IRedisEntity
    {
        private readonly IRedisRepository<TEntity> _redis;
        private readonly IMapper _mapper;

        protected RedisEntityController(IRedisRepository<TEntity> redis, IMapper mapper)
        {
            _redis = redis;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TEntity>> GetById(string id)
        {
            var item = await _redis.GetAsync(id);

            return Ok(item ?? (TEntity)Activator.CreateInstance(typeof(TEntity), id)!);
        }

        [HttpPost]
        public async Task<ActionResult<TEntity>> UpdateOrCreate([FromBody] TDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);

            var saved = await _redis.UpdateOrCreateAsync(dto.Id, entity);

            return Ok(saved);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete([FromRoute] string id)
            => await _redis.DeleteAsync(id)
                ? Ok($"{typeof(TEntity).Name} deleted successfully.")
                : NotFound($"Item with ID '{id}' not found.");
    }
}