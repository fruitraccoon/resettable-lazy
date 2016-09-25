using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ResettableLazy.Cs.Tests
{
    public static class ResettableLazySpec
    {
        private const int TestResult = 42;

        public class TheValuePropertyShould
        {
            [Fact]
            public void ReturnTheFactoryResultWhenCalledOnce()
            {
                var rl = ResettableLazy.Create(() => TestResult);
                var result = rl.Value;
                result.Wait();
                Assert.Equal(TestResult, result.Result);
            }

            [Fact]
            public void ReturnTheFactoryResultWhenCalledAfterReset()
            {
                var rl = ResettableLazy.Create(() => Task.FromResult(TestResult));
                rl.Value.Wait();
                rl.Reset();
                var result = rl.Value;
                result.Wait();
                Assert.Equal(TestResult, result.Result);
            }

            [Fact]
            public void CauseOnlySingleCallOfTheFactoryWhenCalledTwice()
            {
                var count = 0;
                var rl = new ResettableLazy<int>(() =>
                {
                    count++;
                    return TestResult;
                });
                rl.Value.Wait();
                rl.Value.Wait();
                Assert.Equal(1, count);
            }

            [Fact]
            public void CauseOnlySingleCallOfTheFactoryWhenCalledByMultipleThreads()
            {
                var count = 0;
                var rl = new ResettableLazy<int>(() =>
                {
                    return Task.Run(() =>
                    {
                        count++;
                        return TestResult;
                    });
                });
                Enumerable.Repeat(string.Empty, 20).AsParallel().ToList().ForEach(_ => rl.Value.Wait());
                Assert.Equal(1, count);
            }
        }

        public class TheIsValueCreatedPropertyShould
        {
            [Fact]
            public void ReturnFalseWhenCalledBeforeValue()
            {
                var rl = new ResettableLazy<int>(() => TestResult);
                var result = rl.IsValueCreated;
                result.Wait();
                Assert.Equal(false, result.Result);
            }

            [Fact]
            public void ReturnTrueWhenCalledAfterValue()
            {
                var rl = new ResettableLazy<int>(() => TestResult);
                rl.Value.Wait();
                var result = rl.IsValueCreated;
                result.Wait();
                Assert.Equal(true, result.Result);
            }
        }
    }
}
