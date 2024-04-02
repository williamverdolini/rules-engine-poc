using RulesEngine.Models;
using System;
using System.Text;

namespace Core.Services
{
    public class RulesResolver
    {
        private readonly IRulesEngineStore _rulesEngineStore;

        public RulesResolver(IRulesEngineStore rulesEngineStore)
        {
            _rulesEngineStore = rulesEngineStore;
        }

        public async Task<string> Sample(int value)
        {
            const string wfName = "Sample with unnamed input";
            Workflow workflow = await _rulesEngineStore
                .GetWorkflowByName(wfName)
                .ConfigureAwait(false) ?? throw new InvalidOperationException("Incorrect workflow name");

            var re = new RulesEngine.RulesEngine([workflow]);
            var resultList = await re.ExecuteAllRulesAsync(wfName, new
                {
                    val = value
                });

            //Check success for rule
            return ComposeResponseString(resultList);
        }

        public async Task<string> SampleNamed(int minValue, int maxValue)
        {
            const string wfName = "Sample with named";
            Workflow? workflow = await _rulesEngineStore
                .GetWorkflowByName(wfName)
                .ConfigureAwait(false) ?? throw new InvalidOperationException("Incorrect workflow name");
            
            var re = new RulesEngine.RulesEngine([workflow]);
            var rp1 = new RuleParameter("sensor", new
            {
                minValue,
                maxValue,
            });
            var resultList = await re.ExecuteAllRulesAsync(wfName, rp1);

            //Check success for rule
            return ComposeResponseString(resultList);
        }


        public async Task<string> Multiple(string group, string status)
        {
            const string wfName = "TaskType_1.OnInit";
            Workflow? workflow = await _rulesEngineStore
                .GetWorkflowByName(wfName)
                .ConfigureAwait(false) ?? throw new InvalidOperationException("Incorrect workflow name");
            
            var re = new RulesEngine.RulesEngine([workflow]);
            var parameters = new List<RuleParameter>()
            {
                new("user", new { group }),
                new("item", new { status }),
            };
            var resultList = await re.ExecuteAllRulesAsync(wfName, parameters.ToArray());

            //Check success for rule
            return ComposeResponseString(resultList);
        }

        public async Task<string> ApplyLifeCycleRules(string ruleOwner, string phase, string group, string status)
        {
            string wfName = $"{ruleOwner}.{phase}";
            Workflow? workflow = await _rulesEngineStore
                .GetWorkflowByName(wfName)
                .ConfigureAwait(false) ?? throw new InvalidOperationException("Incorrect workflow name");
            
            var re = new RulesEngine.RulesEngine([workflow]);
            var parameters = new List<RuleParameter>()
            {
                new("user", new { group }),
                new("item", new { status }),
            };
            var resultList = await re.ExecuteAllRulesAsync(wfName, parameters.ToArray());

            //Check success for rule
            return ComposeResponseString(resultList);
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
                    ruleName += childRule == null ? "" :  " > " + childRule.RuleName;
                    outputMsg += childRule?.SuccessEvent;
                }

                str.AppendLine(@$"Rule - {ruleName}, 
                IsSuccess - {result.IsSuccess}
                Event - {outputMsg}
                ExceptionMessage: {result.ExceptionMessage}
                Output: {result.ActionResult.Output?.ToString()}"
                );
            }
            return str.ToString();
        }
    }
}
