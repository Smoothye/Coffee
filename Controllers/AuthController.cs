using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WeddingPlannerApp.Data;

namespace WeddingPlannerApp.Controllers;

[Route("auth")]
public class AuthController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager
) : Controller
{
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return LoginFailure("missing-fields", email: email, returnUrl: returnUrl);

        var result = await signInManager.PasswordSignInAsync(
            email.Trim(),
            password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (!result.Succeeded)
            return LoginFailure("invalid-login", email: email, returnUrl: returnUrl);

        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user != null && await userManager.IsInRoleAsync(user, "Admin"))
            return LoginSuccess("~/admin");

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LoginSuccess(returnUrl);

        return LoginSuccess("~/dashboard");
    }

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        [FromForm] string firstName,
        [FromForm] string lastName,
        [FromForm] string email,
        [FromForm] string password)
    {
        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
            return LoginFailure("missing-fields", register: true, email: email);

        var normalizedEmail = email.Trim();
        var existingUser = await userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
            return LoginFailure("duplicate-account", register: true, email: normalizedEmail);

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = true,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim()
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var code = result.Errors.Any(error => error.Code.Contains("Password", StringComparison.OrdinalIgnoreCase))
                ? "password-rules"
                : "register-failed";
            return LoginFailure(code, register: true, email: email);
        }

        await userManager.AddToRoleAsync(user, "User");
        await signInManager.SignInAsync(user, isPersistent: false);

        return LoginSuccess("~/onboarding");
    }

    IActionResult LoginFailure(string error, bool register = false, string? email = null, string? returnUrl = null)
    {
        if (WantsJson())
        {
            return Json(new
            {
                succeeded = false,
                error,
                message = ErrorMessage(error),
                mode = register ? "register" : "login",
                email = email?.Trim() ?? "",
                returnUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : ""
            });
        }

        return RedirectToLogin(error, register, email, returnUrl);
    }

    IActionResult LoginSuccess(string redirectUrl)
    {
        if (WantsJson())
        {
            return Json(new
            {
                succeeded = true,
                redirectUrl = Url.Content(redirectUrl)
            });
        }

        return LocalRedirect(redirectUrl);
    }

    bool WantsJson() =>
        string.Equals(Request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

    IActionResult RedirectToLogin(string error, bool register = false, string? email = null, string? returnUrl = null)
    {
        var mode = register ? "register" : "login";
        var query = $"mode={mode}&error={Uri.EscapeDataString(error)}";

        if (!string.IsNullOrWhiteSpace(email))
            query += $"&email={Uri.EscapeDataString(email.Trim())}";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            query += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";

        return LocalRedirect($"~/login?{query}");
    }

    static string ErrorMessage(string error) => error switch
    {
        "invalid-login" => "Maybe the e-mail or password is not correct, or the account does not exist.",
        "missing-fields" => "Please fill in all required fields.",
        "duplicate-account" => "An account with this e-mail already exists. Please sign in instead.",
        "password-rules" => "Password must include uppercase and lowercase letters, a number, and a special character.",
        "register-failed" => "We could not create this account. Please check the details and try again.",
        _ => "Something went wrong. Please try again."
    };
}
