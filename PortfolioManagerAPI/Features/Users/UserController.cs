using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioManagerAPI.Enums;
using PortfolioManagerAPI.Features.Users.Commands.BuyInvestmentProduct;
using PortfolioManagerAPI.Features.Users.Commands.SellInvestmentProduct;
using PortfolioManagerAPI.Features.Users.Queries.GetInvestmentProductStatement;
using PortfolioManagerAPI.Features.Users.Queries.GetInvestmentProductSummary;
using PortfolioManagerAPI.Services;
using System.Security.Claims;

namespace PortfolioManagerAPI.Features.Users;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "regular")]
public class UserController(IMediator mediator, ITokenService tokenService) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ITokenService _tokenService = tokenService;

    [AllowAnonymous]
    [HttpPost("generateToken")]
    public IActionResult GenerateToken([FromQuery] Role role)
    {
        var token = _tokenService.GenerateToken(role);

        return Ok(new { token });
    }

    [HttpGet("statement")]
    public async Task<IActionResult> GetInvestmentProductStatement()
    {
        var userId = int.Parse(User.FindFirstValue("id")!);
        var query = new GetInvestmentProductStatementQuery { UserId = userId };
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetInvestmentProductSummary()
    {
        var userId = int.Parse(User.FindFirstValue("id")!);
        var query = new GetInvestmentProductSummaryQuery { UserId = userId };
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpPost("buy")]
    public async Task<IActionResult> BuyInvestmentProduct([FromBody] BuyInvestmentProductCommand command)
    {
        var userId = int.Parse(User.FindFirstValue("id")!);
        command.UserId = userId;
        var response = await _mediator.Send(command);

        return Ok(response);
    }

    [HttpPost("sell")]
    public async Task<IActionResult> SellInvestmentProduct([FromBody] SellInvestmentProductCommand command)
    {
        var userId = int.Parse(User.FindFirstValue("id")!);
        command.UserId = userId;
        var response = await _mediator.Send(command);

        return Ok(response);
    }
}
