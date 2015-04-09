<%@ Application Language="C#" %>

<script runat="server">

    void Application_Start(object sender, EventArgs e)
    {
        log4net.Config.XmlConfigurator.ConfigureAndWatch(
                new System.IO.FileInfo(
                    AppDomain.CurrentDomain.SetupInformation.ApplicationBase +
                    "log4net.config"));
    }

</script>
