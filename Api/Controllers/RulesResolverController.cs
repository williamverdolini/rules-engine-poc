using Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("rules")]
    [ApiController]
    public class RulesResolverController : ControllerBase
    {
        private readonly ILogger<RulesResolverController> _logger;
        private readonly RulesResolver _rulesResolver;
        private readonly BusinessRulesResolver _businessRulesResolver;
        private readonly InMemoryProvider _inMemoryProvider;

        public RulesResolverController(
            ILogger<RulesResolverController> logger,
            RulesResolver rulesResolver,
            BusinessRulesResolver businessRulesResolver,
            InMemoryProvider inMemoryProvider)
        {
            _logger = logger;
            _rulesResolver = rulesResolver;
            _businessRulesResolver = businessRulesResolver;
            _inMemoryProvider = inMemoryProvider;
            SetupFallbackInputs();
        }

        [Route("sample/unnamed/{val}")]
        [HttpGet]
        public async Task<string> SampleUnnamed(int val)
        {
            return await _rulesResolver.Sample(val);
        }

        [Route("sample/named/{minValue}/{maxValue}")]
        [HttpGet]
        public async Task<string> SampleNamed(int minValue, int maxValue)
        {
            return await _rulesResolver.SampleNamed(minValue, maxValue);
        }

        [Route("sample/multiple/{group}/{status}")]
        [HttpGet]
        public async Task<string> Multiple(string group, string status)
        {
            return await _rulesResolver.Multiple(group, status);
        }

        [Route("sample/apply-rules/{id}/{phase}")]
        [HttpGet]
        public async Task<RulesOutput> ApplyRules(string id, string phase)
        {
            return await _businessRulesResolver.ApplyLifeCycleRulesAsync(id, phase, null!);
        }

        [Route("sample/apply-rule/{id}/{phase}/{ruleName}")]
        [HttpPost]
        public async Task<RulesOutput> ApplyRule(string id, string phase, string ruleName, [FromBody] dynamic jsonObj)
        {
            var input = new RuleInput("input", jsonObj);
            return await _businessRulesResolver.ApplyLifeCycleRuleAsync(id, phase, ruleName, input);
        }

        [Route("sample/save-object/{id}")]
        [HttpPost]
        public bool SaveInMemoryObj(string id, [FromBody] dynamic jsonObj)
        {
            _inMemoryProvider.UpsertInput(id, jsonObj);
            return true;
        }

        private void SetupFallbackInputs()
        {
            var user = _inMemoryProvider.GetObjectInput("user");
            if (user == null)
            {
                _inMemoryProvider.UpsertInput("user", new
                {
                    fullName = "William Verdolini",
                    group = "AM"
                });
            }
            var item = _inMemoryProvider.GetObjectInput("item");
            if (item == null)
            {
                _inMemoryProvider.UpsertInput("item", new
                {
                    status = "WorkTaskStatus_1"
                });
            }
        }
    }
}
