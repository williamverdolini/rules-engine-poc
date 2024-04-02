namespace Core.Services
{
    public class BusinessRulesResolver
    {
        private readonly RulesEngineWrapper _rulesEngineWrapper;
        private readonly InMemoryProvider _inMemoryProvider;

        public BusinessRulesResolver
            (RulesEngineWrapper rulesEngineWrapper,
            InMemoryProvider inMemoryProvider)
        {
            _rulesEngineWrapper = rulesEngineWrapper;
            _inMemoryProvider = inMemoryProvider;
        }

        public Task<RulesOutput> ApplyLifeCycleRulesAsync(string id, string phase, string field)
        {
            // from Id we recover Ancestors 
            // TODO: maybe we should loop for all the ancestors
            string[] ancestors = [id, "TaskType_1"];
            string owner = "TaskType_1";
            var user = _inMemoryProvider.GetObjectInput("user");
            var item = _inMemoryProvider.GetObjectInput("item");
            return _rulesEngineWrapper
                .ApplyLifeCycleRulesAsync(owner, phase, user, item);
        }

        public Task<RulesOutput> ApplyLifeCycleRuleAsync(string id, string phase, string ruleName, RuleInput input)
        {
            // from Id we recover Ancestors 
            // TODO: maybe we should loop for all the ancestors
            string owner = "TaskType_1";
            return _rulesEngineWrapper
                .ApplyLifeCycleRuleAsync(owner, phase, ruleName, [input]);
        }
    }

    public class User
    {
        public User(string name, string surname, string group)
        {
            Name = name;
            Surname = surname;
            Group = group;
        }

        public string Name { get; }
        public string Surname { get; }
        public string Group { get; }
    }

    public class WorkItem
    {
        public WorkItem(string title, string status)
        {
            Title = title;
            Status = status;
        }

        public string Title { get; }
        public string Status { get; }
    }

    public static class InputMapperExtensions
    {
        public static RuleInput ToInput(this User user)
        {
            return new RuleInput("user", user);
        }

        public static RuleInput ToInput(this WorkItem item)
        {
            return new RuleInput("item", item);
        }

    }
}
