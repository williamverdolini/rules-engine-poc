using Newtonsoft.Json;
using RulesEngine.Models;
using System.Text;

namespace Core.Services
{
    public class RulesEngineWrapper(IRulesEngineStore rulesEngineStore)
    {
        public async Task<RulesOutput> ApplyLifeCycleRulesAsync(string ruleOwner, string phase, params RuleInput[]? inputs)
        {
            string wfName = $"{ruleOwner}.{phase}";
            Workflow workflow = await rulesEngineStore
                .GetWorkflowByName(wfName)
                .ConfigureAwait(false) ?? throw new InvalidOperationException($"Incorrect workflow name: {wfName}");

            var re = new RulesEngine.RulesEngine([workflow]);
            var resultList = await re
                .ExecuteAllRulesAsync(wfName, inputs!.ToParameters())
                .ConfigureAwait(false);

            return ToRuleOutput(ruleOwner, resultList);
        }

        public async Task<RulesOutput> ApplyLifeCycleRuleAsync(string ruleOwner, string phase, string ruleName, params RuleInput[]? inputs)
        {
            string wfName = $"{ruleOwner}.{phase}";
            Workflow workflow = await rulesEngineStore
                .GetWorkflowByName(wfName)
                .ConfigureAwait(false) ?? throw new InvalidOperationException($"Incorrect workflow name: {wfName}");

            var re = new RulesEngine.RulesEngine([workflow]);
            var resultList = await re
                .ExecuteActionWorkflowAsync(wfName, ruleName, inputs!.ToParameters())
                .ConfigureAwait(false);
            
            return ToRuleOutput(ruleOwner, resultList.Results);
        }

        private static string ComposeResponseString(List<RuleResultTree> resultList)
        {
            var str = new StringBuilder();
            foreach (var result in resultList)
            {
                var ruleName = result.Rule.RuleName;
                var outputMsg = result.Rule.SuccessEvent;
                if (result.ChildResults?.Any() == true)
                {
                    var childRule = result.ChildResults.FirstOrDefault(r => r.IsSuccess)?.Rule;
                    ruleName += childRule == null ? "" : " > " + childRule.RuleName;
                    outputMsg += childRule?.SuccessEvent;
                }

                str.AppendLine(@$"Rule - {ruleName}, 
                IsSuccess - {result.IsSuccess}
                Event - {outputMsg}
                ExceptionMessage: {result.ExceptionMessage}
                Output: {result.ActionResult?.Output?.ToString()}"
                );
            }
            return str.ToString();
        }

        private static RulesOutput ToRuleOutput(string ruleOwner, List<RuleResultTree> resultList)
        {
            RulesOutput res = new();
            res.DebugInfo = ComposeResponseString(resultList);
            foreach (RuleResultTree result in resultList)
            {
                FieldOutput field = new()
                {
                    Field = result.Rule.RuleName,
                };
                if (result.ChildResults?.Any() == true)
                {
                    var childRule = result.ChildResults.FirstOrDefault(r => r.IsSuccess)?.Rule;
                    if (!string.IsNullOrEmpty(childRule?.SuccessEvent))
                    {
                        field.RuleName = childRule.RuleName;
                        var status = JsonConvert.DeserializeObject<FieldStatus>(childRule.SuccessEvent);
                        if (status != null)
                        {
                            field.Status = status.Status;
                            field.ValidationRule = status.Validate;
                            field.Message = status.Message;
                        }
                    }
                }
                else
                {
                    FieldStatus status = null;
                    if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Rule.SuccessEvent))
                    {
                        status = JsonConvert.DeserializeObject<FieldStatus>(result.Rule.SuccessEvent);
                    }
                    else if (!result.IsSuccess && !string.IsNullOrWhiteSpace(result.Rule.ErrorMessage))
                    {
                        status = JsonConvert.DeserializeObject<FieldStatus>(result.Rule.ErrorMessage);
                    }
                    if (status != null)
                    {
                        field.Status = status.Status;
                        field.ValidationRule = status.Validate;
                        field.Message = status.Message;
                    }
                }
                res.Fields.Add(field);
            }
            return res;
        }
    }

    public record RuleInput(string Name, object Value);

    public class RulesOutput
    {
        public List<FieldOutput> Fields { get; set; } = [];
        public string? DebugInfo { get; set; }
    }

    public class FieldOutput
    {
        public required string Field { get; set; }
        public string? RuleName { get; set; }
        public string? Message { get; set; }
        public string? Status { get; set; }
        public string? ValidationRule { get; set; }
    }

    public class FieldStatus
    {
        public string? Status { get; set; }
        public string? Validate { get; set; }
        public string? Message { get; set; }
    }

    // Create an extension method for the Person record
    public static class RulesExtensions
    {
        public static RuleParameter ToParameter(this RuleInput input)
        {
            return new RuleParameter(input.Name, input.Value);
        }

        public static RuleParameter[] ToParameters(this RuleInput[] inputs)
        {
            return inputs?.Select(i => i.ToParameter())?.ToArray() ?? [];
        }
    }
}
