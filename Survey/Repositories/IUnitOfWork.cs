using System;
using System.Threading.Tasks;

namespace Survey.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ISurveyRepository Surveys { get; }
        // Add other repositories here, e.g.:
        // IUserRepository Users { get; }

        Task<int> CompleteAsync();
    }
}
