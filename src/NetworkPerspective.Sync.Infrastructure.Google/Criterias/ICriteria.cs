using System.Collections.Generic;

using Google.Apis.Admin.Directory.directory_v1.Data;

namespace NetworkPerspective.Sync.Infrastructure.Google.Criterias
{
    internal interface ICriteria
    {
        IList<User> MeetCriteria(IList<User> users);
    }
}