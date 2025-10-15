using Microsoft.AspNetCore.Components;

namespace WarriorExperiment.App.Components;

/// <summary>
/// Base component for all Warrior Experiment components
/// </summary>
public class WaBaseComponent : ComponentBase
{
    /// <summary>
    /// Specifies additional custom attributes that will be rendered by the component.
    /// </summary>
    /// <value>The attributes.</value>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
    
    /// <summary>
    /// Gets or sets the component ID. If not provided, an automatic ID will be generated.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }
    
    protected string CSSClass { get; set; } = string.Empty;

    /// <summary>
    /// Gets the component CSS class.
    /// </summary>
    protected virtual string GetComponentCssClass()
    {
        return "";
    }

    /// <summary>
    /// Gets the combined CSS class from component, attributes, and custom CSS
    /// </summary>
    protected string GetCssClass()
    {
        if (Attributes != null && Attributes.TryGetValue("class", out var @class) && !string.IsNullOrEmpty(Convert.ToString(@class)))
        {
            return $"{GetComponentCssClass()} {@class} {CSSClass}".Trim();
        }

        return $"{GetComponentCssClass()} {CSSClass}".Trim();
    }
    
    /// <summary>
    /// Gets the component ID. If not provided, generates an automatic ID.
    /// </summary>
    protected string GetComponentId()
    {
        if (!string.IsNullOrEmpty(Id))
            return Id;
            
        // Generate automatic ID based on component type
        var typeName = GetType().Name.ToLowerInvariant();
        return $"{typeName}_{Guid.NewGuid():N}";
    }

    protected override void OnInitialized()
    {
        // Ensure ID is set
        Id ??= GetComponentId();
        base.OnInitialized();
    }
}