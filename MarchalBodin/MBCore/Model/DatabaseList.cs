using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MBCore.Model
{
    public class DatabaseList : List<Database>
    {
        public DatabaseList()
        {
            // BODIN
            this.Add(new Database()
            {
                id = 1,
                name = "BODIN",
                server = "SRVSQL02.MARCHAL-BODIN.LOCAL",
                sageDir = "\\\\SRVAD02\\GESTION\\_SOCIETES",
                email = "langon@marchal-bodin.fr",
                isMag = true
            });

            // SMO
            this.Add(new Database()
            {
                id = 2,
                name = "SMO",
                server = "SRVSQL01.MARCHAL-BODIN.LOCAL",
                sageDir = "\\\\SRVAD01\\GESTION\\_SOCIETES",
                email = "bordeaux@marchal-bodin.fr",
                isMag = true
            });

            // SOBRIGIR
            this.Add(new Database()
            {
                id = 3,
                name = "SOBRIGIR",
                server = "SRVSQL02.MARCHAL-BODIN.LOCAL",
                sageDir = "\\\\SRVAD02\\GESTION\\_SOCIETES",
                email = "podensac@marchal-bodin.fr",
                isMag = true
            });

            // SOGEDIS
            this.Add(new Database()
            {
                id = 4,
                name = "SOGEDIS",
                server = "SRVSQL02.MARCHAL-BODIN.LOCAL",
                sageDir = "\\\\SRVAD02\\GESTION\\_SOCIETES",
                email = "andernos@marchal-bodin.fr",
                isMag = true
            });

            // PPJ
            this.Add(new Database()
            {
                id = 5,
                name = "PPJ",
                server = "SRVSQL02.MARCHAL-BODIN.LOCAL",
                sageDir = "\\\\SRVAD02\\GESTION\\_SOCIETES",
                email = "contact@ppjdistribution.fr",
                isMag = true
            });

            // CDA
            this.Add(new Database()
            {
                id = 6,
                name = "CDA",
                server = "SRVSQL01.MARCHAL-BODIN.LOCAL",
                sageDir = "\\\\SRVAD01\\GESTION\\_SOCIETES",
                email = "cda@marchal-bodin.fr",
                isMag = true
            });

            // TARIF
            this.Add(new Database()
            {
                id = 0,
                name = "TARIF",
                server = "SRVSQL01.MARCHAL-BODIN.LOCAL",
                sageDir = "\\\\SRVAD01\\GESTION\\_SOCIETES",
                email = "si@marchal-bodin.fr",
                isMag = false
            });

            bool isDbLocal = (Directory.Exists("C:\\SAGEDEV"));
            string emailPath = "C:\\SAGEDEV\\email.txt";
            string emailDev = isDbLocal && File.Exists(emailPath) ? File.ReadAllLines(emailPath).First() : "si@marchal-bodin.fr";
            string serverDev = (isDbLocal) ? "LOCALHOST" : "SRVSQL04.MARCHAL-BODIN.LOCAL";

            // BODINDEV
            this.Add(new Database()
            {
                id = 9001,
                name = "BODINDEV",
                server = serverDev,
                sageDir = (isDbLocal) ? "C:\\SAGEDEV\\BODINDEV" : "\\\\SRVSQL04\\SI\\Sage\\DEV\\BODINDEV",
                email = emailDev,
                isMag = true
            });

            // SMODEV
            this.Add(new Database()
            {
                id = 9002,
                name = "SMODEV",
                server = serverDev,
                sageDir = (isDbLocal) ? "C:\\SAGEDEV\\SMODEV" : "\\\\SRVSQL04\\SI\\Sage\\DEV\\SMODEV",
                email = emailDev,
                isMag = true
            });

            // TARIFDEV
            this.Add(new Database()
            {
                id = 9000,
                name = "TARIFDEV",
                server = serverDev,
                sageDir = (isDbLocal) ? "C:\\SAGEDEV\\TARIFDEV" : "\\\\SRVSQL04\\SI\\Sage\\DEV\\TARIFDEV",
                email = emailDev,
                isMag = false
            });
        }
    }
}
