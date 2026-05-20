namespace WeddingPlannerApp.Validation;
using System.ComponentModel.DataAnnotations;

public sealed class NotInPastAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not DateTime date)
            return true;
        
        return date >= DateTime.Today;
    }
}