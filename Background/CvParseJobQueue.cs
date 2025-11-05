using System.Collections.Concurrent;

namespace ERecruitment.Web.Background;

public record CvParseJob(string StorageToken, Guid ApplicantId, string FileName);

public interface ICvParseJobQueue
{
    void Enqueue(CvParseJob job);
    bool TryDequeue(out CvParseJob? job);
}

public class InMemoryCvParseJobQueue : ICvParseJobQueue
{
    private readonly ConcurrentQueue<CvParseJob> _queue = new();

    public void Enqueue(CvParseJob job) => _queue.Enqueue(job);
    public bool TryDequeue(out CvParseJob? job) => _queue.TryDequeue(out job);
}


