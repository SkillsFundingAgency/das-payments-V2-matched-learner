using System;
using System.Threading.Tasks;
using FluentAssertions;
using MatchedLearnerApi.Types;
using NUnit.Framework;

namespace MatchedLearnerApi.AcceptanceTests.SmokeTests
{
    [TestFixture]
    public class RequestToGet404Works
    {
        [Test]
        public async Task RequestWithBadData_Should_Return404()
        {
            var request = new Request();
            Func<Task> act = request.Awaiting(async x => await x.Handle(0, 0));
            act.Should().Throw<Exception>()
                .WithMessage("404");
        }
    }
}
