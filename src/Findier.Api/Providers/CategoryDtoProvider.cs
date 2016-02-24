using System.Data.Entity;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Findier.Api.Helpers;
using Findier.Api.Infrastructure;
using Findier.Api.Models;
using Findier.Api.Models.DataTransfer;

namespace Findier.Api.Providers
{
    public class CategoryDtoProvider : IDtoProvider<Category, DtoCategory>
    {
        private readonly AppDbContext _dbContext;

        public CategoryDtoProvider(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DtoCategory> CreateAsync(Category entry)
        {
            var dto = new DtoCategory(entry);

            var language = LanguageHelper.GetThreadLanguage();

            if (entry.Language != language)
            {
                var translation = await _dbContext.Entry(entry)
                    .Collection(p => p.Translations)
                    .Query()
                    .FirstOrDefaultAsync(p => p.Language == language);
                if (translation != null)
                {
                    dto.Title = translation.Title;
                    dto.Description = translation.Description;
                }
            }

            return dto;
        }
    }
}