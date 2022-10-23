using System.Threading;
using System.Threading.Tasks;

namespace GoC
{
	public interface IUsable
	{
		Task Use(CancellationToken executionCt);
	}
}
