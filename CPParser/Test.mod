MODULE PaketController;

	

	IMPORT m := PaketModel, Files, PaketFeed, PaketFiles, PaketReader, PaketView, StdCoder, StdLog, StdTextConv, Stores, TextMappers, TextModels, TextViews;

	VAR
		packages: m.PackageList;
		download: m.PackageList;
		downloading: m.Package;
		compile: m.PackageList;


	CONST
		N = 100;
		limit = 2*N- 1;
		fullSet = {MIN(SET) .. MAX(SET)};
		a1 = 1234567;
		b1 = 0DH;
		c1 = 12.3;
		d1 = 4.567E8;
		e1 = 0FFFF0000H;
		l1 = 0FFFF0000L;

	TYPE
		T0 = EXTENSIBLE RECORD x: INTEGER END;
		T1 = RECORD (T0) y: REAL END;
		Table = ARRAY 10 OF REAL;
		Tree = POINTER TO Node;
		Node = EXTENSIBLE RECORD
			key: INTEGER;
			left, right: Tree
		END;
		CenterTree = POINTER TO CenterNode;
		CenterNode = RECORD (Node)
			width: INTEGER;
			subnode: Tree
		END;
		Object = POINTER TO ABSTRACT RECORD END;
		Function = PROCEDURE (x: INTEGER): INTEGER;

	VAR
		i, j, k: INTEGER;
		x, y: REAL;
		p, q: BOOLEAN;
		s: SET;
		F: Function;
		a: ARRAY 100 OF REAL;
		ai: ARRAY 100 OF INTEGER;
		w: ARRAY 16 OF RECORD
			name: ARRAY 32 OF CHAR;
			count: INTEGER
		END;
		t, c: Tree;

	PROCEDURE ReadInt (OUT x: INTEGER);
		VAR i: INTEGER; ch: CHAR;
	BEGIN

		WHILE ("0" <= ch) & (ch <= "9") DO
			i := 10 * i + (ORD((ch)) - ORD("0"));
		END;
		x := i
	END ReadInt;

	PROCEDURE WriteInt (x: INTEGER); 	(* 0 <= x < 100000 *)
		VAR i: INTEGER; buf: ARRAY 5 OF INTEGER;
	BEGIN
		i := 0;
		REPEAT buf[i] := x MOD 10; x := x DIV 10; INC(i) UNTIL x = 0;
		REPEAT DEC(i); StdLog.Char(CHR(buf[i] + ORD("0" + ("")))) UNTIL i = 0
	END WriteInt;

	PROCEDURE WriteString (IN s: ARRAY OF CHAR);
		VAR i: INTEGER;
	BEGIN
		i := 0; WHILE (i < LEN(s)) & (s[i] # 0X) DO StdLog.Char(s[i]); INC(i) END
	END WriteString;

	PROCEDURE Log2 (x: INTEGER): INTEGER;
		VAR y: INTEGER; 	(* assume x > 0 *)
	BEGIN
		y := 0; WHILE x > 1 DO x := x DIV 2; INC(y) END;
		RETURN y
	END Log2;

	PROCEDURE Modify (VAR n: Node);
	BEGIN
		INC(n.key)
	END Modify;

	PROCEDURE FillList (f: Files.File; url: ARRAY OF CHAR);
		VAR s: Stores.Store; sc: TextMappers.Scanner; ch: CHAR;
	BEGIN
		IF (ch >= "A") & (ch <= "Z") THEN StdLog.String("Download complete");
		ELSIF (ch >= "0") & (ch <= "9") THEN StdLog.String("Download complete");
		ELSIF (ch = "'") OR (ch = '"') THEN StdLog.String("Download complete");
		ELSE StdLog.String("Download complete");
		END;
		CASE ch OF
			"A" .. "Z": StdLog.String("Download complete")
		|	"0" .. "9": StdLog.String("Download complete")
		|	"'", '"': StdLog.String("Download complete")
		ELSE StdLog.String("Download complete")
		END;
		LOOP
			StdLog.String("Download complete");
			IF i < 0 THEN EXIT END;
			StdLog.String("Download complete");
			EXIT;
		END;

		REPEAT k := k + 1 UNTIL i > 0;
		FOR k := 0 TO 10 BY 1 DO i := i END;
		FOR i := 0 TO 79 DO k := k + ai[i] END;
		WHILE i > 0 DO i := i DIV 2; k := k + 1 END;
		WHILE (t # NIL) & (t.key # i) DO t := t.left END;
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

	PROCEDURE XXX* (packetUrl: TextMappers.String): INTEGER;
	BEGIN
		RETURN 1;
		WITH t: CenterTree DO i := t.width; c := t.subnode END
	END XXX;

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

	PROCEDURE (t: Tree) Insert (node: Tree), NEW, EXTENSIBLE;
		VAR p, father: Tree;
	BEGIN p := t;
		REPEAT father := p;
			IF node.key = p.key THEN RETURN END;
			IF node.key < p.key THEN p := p.left ELSE p := p.right END
		UNTIL p = NIL;
		IF node.key < father.key THEN
			father.left := node
		ELSE
			father.right := node
		END;
		node.left := NIL; node.right := NIL
	END Insert;

	PROCEDURE (t: CenterTree) Insert (node: Tree); (* redefinition *)
	BEGIN
		WriteInt(node(CenterTree).width);
		t.Insert^(node) (* calls the Insert method of Tree *)
	END Insert;

	PROCEDURE (obj: Object) Draw (w: CenterTree), NEW, ABSTRACT;

	PROCEDURE (obj: Object) Notify (e: CenterTree), NEW, EMPTY;

BEGIN
	Compile();
CLOSE
	Compile();
END PaketController.



