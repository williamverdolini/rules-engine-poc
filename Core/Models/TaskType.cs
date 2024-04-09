using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class TaskType
    {
        public string Id { get; set; }

    }

    public class FieldRule
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; }

        public string FieldName { get; set; } = string.Empty;

        public RuleLifeCyclePhase Phase { get; set; }

        public string Expression { get; set; }

        public RuleResult RuleResult { get; set; }
    }

    public enum RuleLifeCyclePhase
    {
        OnInit,
        OnChange,
        OnValidate,
    }

    public record RuleResult(FieldStatus Status, string? Message, string? ValidationRuleName);

    public enum FieldStatus
    {
        Hidden,
        ReadOnly,
        Edit
    }
}

