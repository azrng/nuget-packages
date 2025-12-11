using Common.YuQueSdk.Dto;
using Common.YuQueSdk.Dto.Doc;
using Common.YuQueSdk.Dto.Repository;
using Common.YuQueSdk.Dto.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.YuQueSdk
{
    ///<inheritdoc cref="IYuQueHelper"/>
    public class YuQueHelper : IYuQueHelper
    {
        private readonly IYuQueApi _yuQueApi;

        public YuQueHelper(IYuQueApi yuQueApi)
        {
            _yuQueApi = yuQueApi;
        }

        public async Task<YuQueResult<GetUserResult>> GetUsersAsync(string login)
        {
            return await _yuQueApi.GetUsersAsync(login);
        }

        public async Task<YuQueResult<GetUserResult>> GetUsersAsync(int id)
        {
            return await _yuQueApi.GetUsersAsync(id);
        }

        public async Task<YuQueResult<GetDocsDetailsResponse>> GetReposDocsAsync(long @namespace, string slug)
        {
            return await _yuQueApi.GetReposDocDetailsAsync(@namespace, slug);
        }

        public async Task<YuQueResult<List<GetUserRepositoryResult>>> GetRepoListByLoginNameAsync(string login)
        {
            return await _yuQueApi.GetRepoListByLoginNameAsync(login);
        }

        public async Task<YuQueResult<IEnumerable<GetRepositoryDocResult>>> GetRepositoryDocListAsync(string @namespace)
        {
            return await _yuQueApi.GetRepositoryDocListAsync(@namespace);
        }

        public async Task<YuQueResult<List<RepositoryDocTopic>>> GetRepoTopicListAsync(long repositoryId)
        {
            return await _yuQueApi.GetRepoTopicListAsync(repositoryId);
        }
    }
}