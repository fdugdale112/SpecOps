using FluentAssertions;
using static SpecOps.Tests.Specs;

namespace SpecOps.Tests;

public class SpecificationTests
{
    private static readonly TestEntity John = new("John Doe", "john@example.com", 30);

    public class IndividualSpecTests
    {
        [Fact]
        public void ByEmail_MatchingEmail_ReturnsTrue()
            => ByEmail("john@example.com").IsSatisfiedBy(John).Should().BeTrue();

        [Fact]
        public void ByEmail_DifferentEmail_ReturnsFalse()
            => ByEmail("other@example.com").IsSatisfiedBy(John).Should().BeFalse();

        [Fact]
        public void ByEmail_IsCaseInsensitive()
            => ByEmail("JOHN@EXAMPLE.COM").IsSatisfiedBy(John).Should().BeTrue();

        [Fact]
        public void NameContaining_Match_ReturnsTrue()
            => NameContaining("John").IsSatisfiedBy(John).Should().BeTrue();

        [Fact]
        public void NameContaining_NoMatch_ReturnsFalse()
            => NameContaining("Alice").IsSatisfiedBy(John).Should().BeFalse();

        [Fact]
        public void OlderThan_OlderEntity_ReturnsTrue()
            => OlderThan(25).IsSatisfiedBy(John).Should().BeTrue();

        [Fact]
        public void OlderThan_YoungerEntity_ReturnsFalse()
            => OlderThan(35).IsSatisfiedBy(John).Should().BeFalse();

        [Fact]
        public void IsNotSatisfiedBy_InvertsResult()
            => ByEmail("john@example.com").IsNotSatisfiedBy(John).Should().BeFalse();
    }

    public class AndCompositionTests
    {
        [Fact]
        public void BothMatch_ReturnsTrue()
        {
            var spec = NameContaining("John").And(ByEmail("john@example.com"));
            spec.IsSatisfiedBy(John).Should().BeTrue();
        }

        [Fact]
        public void OnlyOneMatches_ReturnsFalse()
        {
            var spec = NameContaining("John").And(ByEmail("other@example.com"));
            spec.IsSatisfiedBy(John).Should().BeFalse();
        }
    }

    public class OrCompositionTests
    {
        [Fact]
        public void OneMatches_ReturnsTrue()
        {
            var spec = NameContaining("Alice").Or(ByEmail("john@example.com"));
            spec.IsSatisfiedBy(John).Should().BeTrue();
        }

        [Fact]
        public void NeitherMatches_ReturnsFalse()
        {
            var spec = NameContaining("Alice").Or(ByEmail("other@example.com"));
            spec.IsSatisfiedBy(John).Should().BeFalse();
        }
    }

    public class NotCompositionTests
    {
        [Fact]
        public void Negates_MatchingSpec()
            => ByEmail("john@example.com").Not.IsSatisfiedBy(John).Should().BeFalse();

        [Fact]
        public void Negates_NonMatchingSpec()
            => ByEmail("other@example.com").Not.IsSatisfiedBy(John).Should().BeTrue();
    }

    public class FluentChainTests
    {
        [Fact]
        public void AndChain_BothMatch_ReturnsTrue()
        {
            var spec = ByEmail("john@example.com").And().NameContaining("John");
            spec.IsSatisfiedBy(John).Should().BeTrue();
        }

        [Fact]
        public void AndChain_OnlyOneMatches_ReturnsFalse()
        {
            var spec = ByEmail("john@example.com").And().NameContaining("Alice");
            spec.IsSatisfiedBy(John).Should().BeFalse();
        }

        [Fact]
        public void OrChain_OneMatches_ReturnsTrue()
        {
            var spec = ByEmail("other@example.com").Or().NameContaining("John");
            spec.IsSatisfiedBy(John).Should().BeTrue();
        }

        [Fact]
        public void MultipleAndChains()
        {
            var spec = ByEmail("john@example.com")
                .And().NameContaining("John")
                .And().OlderThan(25);

            spec.IsSatisfiedBy(John).Should().BeTrue();
        }
    }

    public class LeftToRightEvaluationTests
    {
        [Fact]
        public void AndThenOr_EvaluatesLeftToRight()
        {
            // (email AND name) OR "Alice" → true
            var spec = ByEmail("john@example.com")
                .And().NameContaining("John")
                .Or().NameContaining("Alice");

            spec.IsSatisfiedBy(John).Should().BeTrue();
        }

        [Fact]
        public void OrThenAnd_EvaluatesLeftToRight()
        {
            // (wrong email OR name) AND correct email
            var spec = ByEmail("other@example.com")
                .Or().NameContaining("John")
                .And().ByEmail("john@example.com");

            // (false OR true) = true, AND true = true
            spec.IsSatisfiedBy(John).Should().BeTrue();
        }

        [Fact]
        public void ExplicitGrouping_DiffersFromChaining()
        {
            // Chained: (true OR false) AND false → false
            var chained = NameContaining("John")
                .Or().ByEmail("other@example.com")
                .And().ByEmail("other@example.com");

            chained.IsSatisfiedBy(John).Should().BeFalse();

            // Grouped: true OR (false AND false) → true
            var grouped = NameContaining("John")
                .Or(ByEmail("other@example.com").And(ByEmail("other@example.com")));

            grouped.IsSatisfiedBy(John).Should().BeTrue();
        }
    }

    public class SpecSuffixStrippingTests
    {
        [Fact]
        public void YoungerThanSpec_GeneratesMethodWithoutSuffix()
            => YoungerThan(35).IsSatisfiedBy(John).Should().BeTrue();

        [Fact]
        public void YoungerThanSpec_WorksInChain()
        {
            var spec = NameContaining("John").And().YoungerThan(35);
            spec.IsSatisfiedBy(John).Should().BeTrue();
        }
    }

    public class QueryableExtensionTests
    {
        [Fact]
        public void WithSpecification_FiltersCorrectly()
        {
            var entities = new[]
            {
                new TestEntity("John", "john@example.com", 30),
                new TestEntity("Alice", "alice@example.com", 25),
                new TestEntity("Bob", "bob@example.com", 40)
            }.AsQueryable();

            var result = entities.WithSpecification(OlderThan(28)).ToList();

            result.Should().HaveCount(2);
            result.Select(e => e.Name).Should().Contain("John").And.Contain("Bob");
        }
    }
}
