# MARK

Markdown documents to PDF

## Usage

```bash
# Run docker image
docker run -d -p "8080:8080" projektanker/mark:latest
# Wait for server to start
sleep 5
# Post multipart/form-data
curl -F "files=@metadata.yaml" -F "files=@template.zip" -o example.pdf http://localhost:8080/api/pdf
```

Required files:
- `template.md`
- `metadata.yaml`

Optional files:
- `document.css` to style your document.
- `header.html` to add a header to your document.
- `footer.html` to add a footer to your document.
- `pagelayout.yaml` to change the layout of your document.Defaults to DIN A4 with 10mm margins.
- `template.html` to change the used html template. Defaults to pandoc HTML5 template (extract template with `pandoc -D html5`)
- other files like images or fonts are also supported.

Any of these files can be in one or more `*.zip` or `*.tar` archives and will be extracted by the server. See `./integration-test` for an advanced example.

## How does it work?

MARK uses [pandoc](https://pandoc.org/) to convert the `template.md` and `metadata.yaml` to an intermediate markdown document. The `template.md` supports the [pandoc template syntax](https://pandoc.org/MANUAL.html#template-syntax).  
The intermediate markdown document is converted to a HTML document using the provided `template.html` file (defaults to pandoc default HTML5 template).

The HTML document is converted to a PDF document by using [Playwright](https://playwright.dev/).  
To add a header or footer provide a `header.html` or `footer.html` file respectively. The header and footer support some classes to inject printing values (see [Playwright HeaderTemplate](https://playwright.dev/dotnet/docs/api/class-page#page-pdf-option-header-template). The document styles are not available in the header and footer and must be added inline or within an `<style>...</style>` tag at the beginning of the file.

## Debug

You can use the `api/html` endpoint to get the intermediate HTML file used to generate the final pdf document.
```bash
# ...
curl -F "files=@metadata.yaml" -F "files=@template.zip" -o example.html http://localhost:8080/api/html
```