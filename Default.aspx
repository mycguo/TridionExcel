<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>http://blog.building-blocks.com/creating-custom-pages-using-the-core-service-in-sdl-tridion-2011</title>
</head>
<body>
    <form id="form1" runat="server">
        <div class="pricing-management">
        <h1 style="float:left;">Virtual Office Pricing Manager</h1>
        <br clear="all" />
    <fieldset>
        <asp:Label ID="goodMessage" runat="server" CssClass="msg-good" Visible="false" />
        <asp:Label ID="badMessage" runat="server" CssClass="msg-bad" Visible="false" />
        <dl>
            <dt>Upload Pricing</dt>
            <dd>
                <strong>Select Spreadsheet (.xls / .xlsx):</strong><br />
                <asp:FileUpload ID="fileUpload" runat="server" /><br /><br />
                <asp:Button ID="btnUpload" runat="server" Text="Upload" 
                    onclick="btnUpload_Click" />
            </dd>
            <dt>Publish</dt>
            <dd>
                <asp:Button ID="btnPublishStaging" CssClass="publish-staging" runat="server" Text="Publish To Staging" />
                <asp:Button ID="btnPublishLive" CssClass="publish-live" runat="server" Text="Publish To Live" />
            </dd>
        </dl>
    </fieldset>
    </div>
    </form>
</body>
</html>
