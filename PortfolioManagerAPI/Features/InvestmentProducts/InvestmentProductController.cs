using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioManagerAPI.Features.InvestmentProducts.Commands.CreateInvestmentProduct;
using PortfolioManagerAPI.Features.InvestmentProducts.Commands.DeleteInvestmentProduct;
using PortfolioManagerAPI.Features.InvestmentProducts.Commands.UpdateInvestmentProduct;
using PortfolioManagerAPI.Features.InvestmentProducts.Queries.GetInvestmentProductById;
using PortfolioManagerAPI.Features.InvestmentProducts.Queries.GetInvestmentProducts;

namespace PortfolioManagerAPI.Features.InvestmentProducts;

[ApiController]
[Route("[controller]")]
public class InvestmentProductController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    [Authorize(Roles = "admin,regular")]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetInvestmentProductsQuery();
        var response = await _mediator.Send(query);

        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetInvestmentProductByIdQuery { Id = id };
        var response = await _mediator.Send(query);

        if (response is null) return NotFound();

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] CreateInvestmentProductCommand command)
    {
        var response = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetById), new { response.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInvestmentProductCommand command)
    {
        command.Id = id;
        await _mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteInvestmentProductCommand { Id = id });
        return NoContent();
    }
}
