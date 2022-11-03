namespace Mark.Web.Pages.Helper;

public class ManyClasses : List<string?>
{
    public override string ToString()
    {
        return string.Join(' ', this
            .Where(value => !string.IsNullOrEmpty(value)));
    }
}
