MODULE PaketController;
	
	IMPORT m := PaketModel, Files, PaketFeed, PaketFiles, PaketReader, PaketView, StdCoder, StdLog, StdTextConv, Stores, TextMappers, TextModels, TextViews;

	CONST
		N = 100;
		limit = 2*N - 1;
		fullSet = {MIN(SET) .. MAX(SET)};	
		a = 1234567;
		b = 0DH;
		c = 12.3;
		d = 4.567E8;
		e = 0FFFF0000H;
		l = 0FFFF0000L;

	VAR
		packages: m.PackageList;
		download: m.PackageList;
		downloading: m.Package;
		compile: m.PackageList;

	TYPE
		T0 = EXTENSIBLE RECORD x: INTEGER END;
		T1 = RECORD (T0) y: REAL END;
		Table = ARRAY 10 OF REAL;
		Tree = POINTER TO Node;
		Node =  EXTENSIBLE RECORD(T0)
			key : INTEGER;
			left, right: Tree
		END;
		CenterTree = POINTER TO CenterNode;
		CenterNode = RECORD (Node)
			width: INTEGER;
			subnode: Tree
		END;
		Object = POINTER TO ABSTRACT RECORD END;
		Function = PROCEDURE (x: INTEGER): INTEGER;

	PROCEDURE FillList (f: Files.File; url: ARRAY OF CHAR);
		VAR s: Stores.Store; sc: TextMappers.Scanner;
	BEGIN
		StdLog.String("Download complete"); StdLog.Ln;
		StdTextConv.ImportUtf8(f, s);
		sc.ConnectTo(s(TextViews.View).ThisModel());
		sc.SetOpts(sc.opts + {TextMappers.returnCtrlChars});
		sc.SetPos(0);
		sc.Scan;
		packages := PaketReader.ReadPackages(sc,  - 1);
		PaketView.Open(packages);
	END FillList;

	PROCEDURE DownloadFeed* ();
	BEGIN
		PaketFeed.DownloadFeed(FillList);
	END DownloadFeed;

	PROCEDURE ^ InstallPackage (f: Files.File; url: ARRAY OF CHAR);

	PROCEDURE Download* (packetUrl: TextMappers.String);
	BEGIN
		downloading := m.FindPackageBy(packages, m.urlTag, packetUrl + "");
		ASSERT(downloading # NIL);
		PaketFeed.DownloadExtension(packetUrl, InstallPackage)
	END Download;

	PROCEDURE Compile* ();
		VAR p: m.Package;
	BEGIN
		(*TODO Sort compile*)
		p := m.PopFromList(compile);
		WHILE p # NIL DO
			PaketFiles.CompilePackage(p);
			p := m.PopFromList(compile);
		END;
	END Compile;

	PROCEDURE InstallPackage (f: Files.File; url: ARRAY OF CHAR);
		VAR text: TextModels.Model; s: Stores.Store; ask: BOOLEAN;
			deps: m.PackageList;
	BEGIN
		StdLog.String("Extracting package "); StdLog.String(downloading.data[m.nameTag]); StdLog.Ln;
		StdTextConv.ImportText(f, s);
		text := s(TextViews.View).ThisModel();
		ask := StdCoder.askOption;
		StdCoder.askOption := FALSE;
		StdCoder.DecodeAllFromText(text, 0, FALSE);
		StdCoder.askOption := ask;
		PaketFiles.SetDependencies(packages, downloading);
		downloading.isInstalled := TRUE;
		PaketFiles.OpenQuickStart(downloading);
		m.AddToList(compile, downloading, m.nameTag);
		deps := downloading.depends;

		WHILE deps # NIL DO
			IF ~deps.package.isInstalled THEN
				m.AddToList(download, deps.package, m.nameTag);
			END;
			deps := deps.next;
		END;

		downloading := m.PopFromList(download);
		IF downloading # NIL THEN
			Download(downloading.data[m.urlTag]);
		ELSE
			Compile();
			PaketView.Open(packages);
		END;

	END InstallPackage;

END PaketController.



