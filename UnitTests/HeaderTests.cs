﻿//
// HeaderTests.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2013-2023 .NET Foundation and Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System.Text;

using MimeKit;

namespace UnitTests {
	[TestFixture]
	public class HeaderTests
	{
		static string ByteArrayToString (byte[] text)
		{
			var builder = new StringBuilder ();

			for (int i = 0; i < text.Length; i++)
				builder.Append ((char) text[i]);

			return builder.ToString ();
		}

		static int GetMaxLineLength (string text)
		{
			int current = 0;
			int max = 0;

			for (int i = 0; i < text.Length; i++) {
				if (text[i] == '\r' && text[i + 1] == '\n')
					i++;

				if (text[i] == '\n') {
					max = Math.Max (max, current);
					current = 0;
				} else {
					current++;
				}
			}

			return max;
		}

		[Test]
		public void TestArgumentExceptions  ()
		{
			var header = new Header ("utf-8", HeaderId.Subject, "This is a subject...");

			Assert.Throws<ArgumentOutOfRangeException> (() => new Header (HeaderId.Unknown, "value"));
			Assert.Throws<ArgumentNullException> (() => new Header (HeaderId.Subject, null));
			Assert.Throws<ArgumentNullException> (() => new Header (null, "value"));
			Assert.Throws<ArgumentException> (() => new Header (string.Empty, "value"));
			Assert.Throws<ArgumentNullException> (() => new Header ("field", null));
			Assert.Throws<ArgumentNullException> (() => new Header ((Encoding) null, HeaderId.Subject, "value"));
			Assert.Throws<ArgumentOutOfRangeException> (() => new Header (Encoding.UTF8, HeaderId.Unknown, "value"));
			Assert.Throws<ArgumentNullException> (() => new Header (Encoding.UTF8, HeaderId.Subject, null));
			Assert.Throws<ArgumentNullException> (() => new Header ((string) null, "field", "value"));
			Assert.Throws<ArgumentNullException> (() => new Header ("utf-8", null, "value"));
			Assert.Throws<ArgumentException> (() => new Header ("utf-8", string.Empty, "value"));
			Assert.Throws<ArgumentNullException> (() => new Header ("utf-8", "field", null));
			Assert.Throws<ArgumentNullException> (() => new Header ((Encoding) null, "field", "value"));
			Assert.Throws<ArgumentNullException> (() => new Header (Encoding.UTF8, null, "value"));
			Assert.Throws<ArgumentException> (() => new Header (Encoding.UTF8, string.Empty, "value"));
			Assert.Throws<ArgumentNullException> (() => new Header (Encoding.UTF8, "field", null));
			Assert.Throws<ArgumentNullException> (() => new Header ((string) null, HeaderId.Subject, "value"));
			Assert.Throws<ArgumentOutOfRangeException> (() => new Header ("utf-8", HeaderId.Unknown, "value"));
			Assert.Throws<ArgumentNullException> (() => new Header ("utf-8", HeaderId.Subject, null));

			// GetValue
			Assert.Throws<ArgumentNullException> (() => header.GetValue ((Encoding) null));
			Assert.Throws<ArgumentNullException> (() => header.GetValue ((string) null));

			// SetValue
			Assert.Throws<ArgumentNullException> (() => header.SetValue (null, Encoding.UTF8, "value"));
			Assert.Throws<ArgumentNullException> (() => header.SetValue (FormatOptions.Default, (Encoding) null, "value"));
			Assert.Throws<ArgumentNullException> (() => header.SetValue (FormatOptions.Default, Encoding.UTF8, null));
			Assert.Throws<ArgumentNullException> (() => header.SetValue (null, "utf-8", "value"));
			Assert.Throws<ArgumentNullException> (() => header.SetValue (FormatOptions.Default, (string) null, "value"));
			Assert.Throws<ArgumentNullException> (() => header.SetValue (FormatOptions.Default, "utf-8", null));
			Assert.Throws<ArgumentNullException> (() => header.SetValue ((Encoding) null, "value"));
			Assert.Throws<ArgumentNullException> (() => header.SetValue (Encoding.UTF8, null));
			Assert.Throws<ArgumentNullException> (() => header.SetValue ((string) null, "value"));
			Assert.Throws<ArgumentNullException> (() => header.SetValue ("utf-8", null));

			// SetRawValue
			Assert.Throws<ArgumentNullException> (() => header.SetRawValue (null));
			Assert.Throws<ArgumentException> (() => header.SetRawValue (Array.Empty<byte> ()));
			Assert.Throws<ArgumentException> (() => header.SetRawValue (Encoding.ASCII.GetBytes ("abc")));
		}

		[Test]
		public void TestCloning ()
		{
			var header = new Header (HeaderId.Comments, "These are some comments.");
			var clone = header.Clone ();

			Assert.AreEqual (header.Id, clone.Id, "The cloned header id does not match.");
			Assert.AreEqual (header.Field, clone.Field, "The cloned header field does not match.");
			Assert.AreEqual (header.Value, clone.Value, "The cloned header value does not match.");
			Assert.AreEqual (header.RawField, clone.RawField, "The cloned header raw field does not match.");
			Assert.AreEqual (header.RawValue, clone.RawValue, "The cloned header raw value does not match.");
		}

		[Test]
		public void TestAddressHeaderFolding ()
		{
			var expected = " Jeffrey Stedfast <jeff@xamarin.com>, \"Jeffrey A. Stedfast\"" + FormatOptions.Default.NewLine +
				"\t<jeff@xamarin.com>, \"Dr. Gregory House, M.D.\"" + FormatOptions.Default.NewLine +
				"\t<house@princeton-plainsboro-hospital.com>" + FormatOptions.Default.NewLine;
			var header = new Header ("To", "Jeffrey Stedfast <jeff@xamarin.com>, \"Jeffrey A. Stedfast\" <jeff@xamarin.com>, \"Dr. Gregory House, M.D.\" <house@princeton-plainsboro-hospital.com>");
			var raw = ByteArrayToString (header.RawValue);

			Assert.IsTrue (raw[raw.Length - 1] == '\n', "The RawValue does not end with a new line.");

			Assert.IsTrue (GetMaxLineLength (raw) < FormatOptions.Default.MaxLineLength, "The RawValue is not folded properly.");
			Assert.AreEqual (expected, raw, "The folded address header does not match the expected value.");
		}

		static readonly string[] ArcAuthenticationResultsHeaderValues = {
			" i=1; lists.example.org;" + FormatOptions.Default.NewLine + "\tspf=pass smtp.mfrom=jqd@d1.example;" + FormatOptions.Default.NewLine + "\tdkim=pass (1024 - bit key) header.i=@d1.example; dmarc=pass",
			" i=2; gmail.example;" + FormatOptions.Default.NewLine + "\tspf=fail smtp.from=jqd@d1.example;" + FormatOptions.Default.NewLine + "\tdkim=fail (512-bit key) header.i=@example.org; dmarc=fail;" + FormatOptions.Default.NewLine + "\tarc=pass (as.1.lists.example.org=pass, ams.1.lists.example.org=pass)",
			" i=3; gmail.example;" + FormatOptions.Default.NewLine + "\tspf=fail smtp.from=jqd@d1.example;" + FormatOptions.Default.NewLine + "\tdkim=fail (512-bit key) header.i=@example.org; dmarc=fail"+ FormatOptions.Default.NewLine + "\t(this-is-a-really-really-really-long-unbroken-comment-that-will-be-on-a-line-by-itself);" + FormatOptions.Default.NewLine + "\tarc=pass (as.1.lists.example.org=pass, ams.1.lists.example.org=pass)"
		};

		[Test]
		public void TestArcAuthenticationResultsHeaderFolding ()
		{
			var header = new Header ("ARC-Authentication-Results", "");

			foreach (var authResults in ArcAuthenticationResultsHeaderValues) {
				header.SetValue (Encoding.ASCII, authResults.Replace (FormatOptions.Default.NewLine + "\t", " ").Trim ());

				var raw = ByteArrayToString (header.RawValue);

				Assert.IsTrue (raw[raw.Length - 1] == '\n', "The RawValue does not end with a new line.");

				Assert.AreEqual (authResults + FormatOptions.Default.NewLine, raw, "The folded ARC-Authentication-Results header does not match the expected value.");
			}
		}

		[Test]
		public void TestMessageIdHeaderFolding ()
		{
			var header = new Header ("Message-Id", string.Format ("<{0}@princeton-plainsboro-hospital.com>", Guid.NewGuid ()));
			var expected = " " + header.Value + FormatOptions.Default.NewLine;
			var raw = ByteArrayToString (header.RawValue);

			Assert.IsTrue (raw[raw.Length - 1] == '\n', "The RawValue does not end with a new line.");

			Assert.AreEqual (expected, raw, "The folded Message-Id header does not match the expected value.");
		}

		[Test]
		public void TestSubjectHeaderFolding ()
		{
			const string expected = " =?utf-8?b?0KLQtdGB0YLQvtCy0YvQuSDQt9Cw0LPQvtC70L7QstC+0Log0L/QuNGB0YzQvNCw?=\n";
			var header = new Header ("Subject", "Тестовый заголовок письма");
			var actual = ByteArrayToString (header.RawValue).Replace ("\r", "");

			Assert.AreEqual (expected, actual);
		}

		static readonly string[] ReceivedHeaderValues = {
			" from thumper.bellcore.com by greenbush.bellcore.com (4.1/4.7)" + FormatOptions.Default.NewLine + "\tid <AA01648> for nsb; Fri, 29 Nov 91 07:13:33 EST",
			" from joyce.cs.su.oz.au by thumper.bellcore.com (4.1/4.7)" + FormatOptions.Default.NewLine + "\tid <AA11898> for nsb@greenbush; Fri, 29 Nov 91 07:11:57 EST",
			" from Messages.8.5.N.CUILIB.3.45.SNAP.NOT.LINKED.greenbush.galaxy.sun4.41" + FormatOptions.Default.NewLine + "\tvia MS.5.6.greenbush.galaxy.sun4_41; Fri, 12 Jun 1992 13:29:05 -0400 (EDT)",
			" from sqhilton.pc.cs.cmu.edu by po3.andrew.cmu.edu (5.54/3.15)" + FormatOptions.Default.NewLine + "\tid <AA21478> for beatty@cosmos.vlsi.cs.cmu.edu; Wed, 26 Aug 92 22:14:07 EDT",
			" from [127.0.0.1] by [127.0.0.1] id <AA21478> with sendmail (v1.8)" + FormatOptions.Default.NewLine + "\tfor <beatty@cosmos.vlsi.cs.cmu.edu>; Wed, 26 Aug 92 22:14:07 EDT",
		};

		[Test]
		public void TestReceivedHeaderFolding ()
		{
			var header = new Header ("Received", "");

			foreach (var received in ReceivedHeaderValues) {
				header.SetValue (Encoding.ASCII, received.Replace (FormatOptions.Default.NewLine + "\t", " ").Trim ());

				var raw = ByteArrayToString (header.RawValue);

				Assert.IsTrue (raw[raw.Length - 1] == '\n', "The RawValue does not end with a new line.");

				Assert.AreEqual (received + FormatOptions.Default.NewLine, raw, "The folded Received header does not match the expected value.");
			}
		}

		[Test]
		public void TestReferencesHeaderFolding ()
		{
			var expected = new StringBuilder ();

			expected.AppendFormat (" <{0}@princeton-plainsboro-hospital.com>", Guid.NewGuid ());
			for (int i = 0; i < 5; i++)
				expected.AppendFormat ("{0}\t<{1}@princeton-plainsboro-hospital.com>", FormatOptions.Default.NewLine, Guid.NewGuid ());

			expected.Append (FormatOptions.Default.NewLine);

			var header = new Header ("References", expected.ToString ());
			var raw = ByteArrayToString (header.RawValue);

			Assert.IsTrue (raw[raw.Length - 1] == '\n', "The RawValue does not end with a new line.");

			Assert.AreEqual (expected.ToString (), raw, "The folded References header does not match the expected value.");
		}

		[Test]
		public void TestDkimSignatureHeaderFolding ()
		{
			var header = new Header ("UTF-8", "DKIM-Signature", "v=1; a=rsa-sha256; c=simple/simple; d=maillist.codeproject.com; s=mail; t=1435835767; bh=tiafHSAvEg4GPJlbkR6e7qr1oydTj+ZXs392TcHwwvs=; h=MIME-Version:From:To:Date:Subject:Content-Type:Content-Transfer-Encoding:Message-Id; b=Qtgo0bWwT0H18CxD2+ey8/382791TBNYtZ8VOLlXxxsbw5fab8uEo53o5tPun6kNx4khmJx/yWowvrCOAcMoqgNO7Hb7JB8NR7eNyOvtLKCG34AfDZyHNcTZHR/QnBpRKHssu5w2CQDUAjKnuGKRW95LCMMX3r924dErZOJnGhs=");
			var expected = " v=1; a=rsa-sha256; c=simple/simple;\n\td=maillist.codeproject.com; s=mail; t=1435835767;\n\tbh=tiafHSAvEg4GPJlbkR6e7qr1oydTj+ZXs392TcHwwvs=;\n\th=MIME-Version:From:To:Date:Subject:Content-Type:Content-Transfer-Encoding:\n\tMessage-Id;\n\tb=Qtgo0bWwT0H18CxD2+ey8/382791TBNYtZ8VOLlXxxsbw5fab8uEo53o5tPun6kNx4khmJx/yWo\n\twvrCOAcMoqgNO7Hb7JB8NR7eNyOvtLKCG34AfDZyHNcTZHR/QnBpRKHssu5w2CQDUAjKnuGKRW95L\n\tCMMX3r924dErZOJnGhs=\n";
			var raw = ByteArrayToString (header.RawValue);

			expected = expected.Replace ("\n", Environment.NewLine);

			Assert.AreEqual (expected, raw, "The RawValue does not match the expected value.");
		}

		[Test]
		public void TestUnstructuredHeaderFolding ()
		{
			var header = new Header ("Subject", "This is a subject value that should be long enough to force line wrapping to keep the line length under the 78 character limit.");
			var raw = ByteArrayToString (header.RawValue);

			Assert.IsTrue (raw[raw.Length - 1] == '\n', "The RawValue does not end with a new line.");

			Assert.IsTrue (GetMaxLineLength (raw) < FormatOptions.Default.MaxLineLength, "The RawValue is not folded properly.");

			var unfolded = Header.Unfold (raw);
			Assert.AreEqual (header.Value, unfolded, "Unfolded header does not match the original header value.");
		}

		[Test]
		public void TestSimpleInternationalizedUnstructuredHeaderFolding ()
		{
			var options = FormatOptions.Default.Clone ();
			string original, folded, unfolded;

			options.International = true;

			original = "This is a subject value that should be long enough to force line wrapping to keep the line length under the 78 character limit.";
			folded = Header.Fold (options, "Subject", original);
			unfolded = Header.Unfold (folded);

			Assert.IsTrue (folded[folded.Length - 1] == '\n', "The folded header does not end with a new line.");
			Assert.IsTrue (GetMaxLineLength (folded) < FormatOptions.Default.MaxLineLength, "The raw header value is not folded properly. ");
			Assert.AreEqual (original, unfolded, "Unfolded header does not match the original header value.");
		}

		[Test]
		public void TestArabicInternationalizedUnstructuredHeaderFolding ()
		{
			var options = FormatOptions.Default.Clone ();
			string original, folded, unfolded;

			options.International = true;

			original = "هل تتكلم اللغة الإنجليزية /العربية؟" + "هل تتكلم اللغة الإنجليزية /العربية؟" + "هل تتكلم اللغة الإنجليزية /العربية؟" + "هل تتكلم اللغة الإنجليزية /العربية؟" + "هل تتكلم اللغة الإنجليزية /العربية؟";
			folded = Header.Fold (options, "Subject", original);
			unfolded = Header.Unfold (folded);

			Assert.IsTrue (folded[folded.Length - 1] == '\n', "The folded header does not end with a new line.");
			Assert.IsTrue (GetMaxLineLength (folded) < FormatOptions.Default.MaxLineLength, "The raw header value is not folded properly. ");
			Assert.AreEqual (original, unfolded, "Unfolded header does not match the original header value.");
		}

		[Test]
		public void TestJapaneseInternationalizedUnstructuredHeaderFolding ()
		{
			var options = FormatOptions.Default.Clone ();
			string original, folded, unfolded;

			options.International = true;

			original = "狂ったこの世で狂うなら気は確かだ。" + "狂ったこの世で狂うなら気は確かだ。" + "狂ったこの世で狂うなら気は確かだ。" + "狂ったこの世で狂うなら気は確かだ。";
			folded = Header.Fold (options, "Subject", original);
			unfolded = Header.Unfold (folded).Replace (" ", "");

			Assert.IsTrue (folded[folded.Length - 1] == '\n', "The folded header does not end with a new line.");
			Assert.IsTrue (GetMaxLineLength (folded) < FormatOptions.Default.MaxLineLength, "The raw header value is not folded properly.");
			Assert.AreEqual (original, unfolded, "Unfolded header does not match the original header value.");
		}

		[Test]
		public void TestReallyLongWordHeaderFolding ()
		{
			const string original = "This is a header value with_a_really_really_really_long_word_that_will_need_to_be_broken_up_in_order_to_fold";
			var options = FormatOptions.Default.Clone ();
			string folded, unfolded;

			folded = Header.Fold (options, "Subject", original);
			unfolded = Header.Unfold (folded).Replace (" _", "_");

			Assert.IsTrue (folded[folded.Length - 1] == '\n', "The folded header does not end with a new line.");
			Assert.IsTrue (GetMaxLineLength (folded) <= FormatOptions.Default.MaxLineLength, "The raw header value is not folded properly.");
			Assert.AreEqual (original, unfolded, "Unfolded header does not match the original header value.");
		}

		[Test]
		public void TestJapaneseUTF8HeaderDecoding ()
		{
			const string input = "Subject: =?UTF-8?B?RndkOiDjgI7jg53jgrHjg6Ljg7Mgzqnjg6vjg5Pjg7zjg7vOseOCteODleOCoeOCpOOCog==?= =?UTF-8?B?44CP44KS44OX44Os44Kk44GV44KM44Gf55qG44GV44G+44G4IDcyMOeorumhnuOBruODneOCseODog==?= =?UTF-8?B?44Oz44GM5Yui44Ge44KN44GE77yBM0RT5pyA5paw44K944OV44OI44Gu44GK44GX44KJ44Gb44Gn44GZ?=";
			const string expected = "Fwd: 『ポケモン Ωルビー・αサファイア』をプレイされた皆さまへ 720種類のポケモンが勢ぞろい！3DS最新ソフトのおしらせです";
			Header header;

			Assert.IsTrue (Header.TryParse (input, out header), "Failed to parse Japanese Subject header.");
			Assert.AreEqual (HeaderId.Subject, header.Id, "HeaderId does not match");
			Assert.AreEqual (expected, header.Value, "Subject values do not match.");
		}

		[Test]
		public void TestJapaneseISO2022JPHeaderDecoding ()
		{
			const string input = "Subject: =?ISO-2022-JP?B?GyRCRnxLXDhsJWEhPCVrJUYlOSVIGyhCICh0ZXN0aW5nIEph?=\n =?ISO-2022-JP?B?cGFuZXNlIGVtYWlscyk=?=";
			const string expected = "日本語メールテスト (testing Japanese emails)";
			Header header;

			Assert.IsTrue (Header.TryParse (input, out header), "Failed to parse Japanese Subject header.");
			Assert.AreEqual (HeaderId.Subject, header.Id, "HeaderId does not match");
			Assert.AreEqual (expected, header.Value, "Subject values do not match.");
		}

		[Test]
		public void TestRawUTF8HeaderDecoding ()
		{
			const string input = "Subject: Fwd: 『ポケモン Ωルビー・αサファイア』をプレイされた皆さまへ 720種類のポケモンが勢ぞろい！3DS最新ソフトのおしらせです";
			const string expected = "Fwd: 『ポケモン Ωルビー・αサファイア』をプレイされた皆さまへ 720種類のポケモンが勢ぞろい！3DS最新ソフトのおしらせです";
			var buffer = Encoding.UTF8.GetBytes (input);
			Header header;

			Assert.IsTrue (Header.TryParse (buffer, out header), "Failed to parse raw UTF-8 Subject header.");
			Assert.AreEqual (HeaderId.Subject, header.Id, "HeaderId does not match");
			Assert.AreEqual (expected, header.Value, "Subject values do not match.");

			Assert.IsTrue (Header.TryParse (buffer, 0, buffer.Length, out header), "Failed to parse raw UTF-8 Subject header.");
			Assert.AreEqual (HeaderId.Subject, header.Id, "HeaderId does not match");
			Assert.AreEqual (expected, header.GetValue ("utf-8"), "Subject values do not match.");
		}

		[Test]
		public void TestParserCanonicalization ()
		{
			Assert.IsTrue (Header.TryParse ("Content-Type: text/plain", out var header), "TryParse");
			Assert.AreEqual ("Content-Type", header.Field, "Field");
			Assert.AreEqual ("text/plain", header.Value, "Value");
			Assert.IsTrue (header.RawValue[header.RawValue.Length - 1] == (byte) '\n', "RawValue should end with a new-line");
		}

		[Test]
		public void TestToHeaderId ()
		{
			HeaderId parsed;
			string name;

			foreach (HeaderId value in Enum.GetValues (typeof (HeaderId))) {
				if (value == HeaderId.Unknown)
					continue;

				name = value.ToHeaderName ().ToUpperInvariant ();
				parsed = name.ToHeaderId ();

				Assert.AreEqual (value, parsed, "Failed to parse the HeaderId value for {0}", value);
			}

			parsed = "X-MadeUp-Header".ToHeaderId ();

			Assert.AreEqual (HeaderId.Unknown, parsed, "Failed to parse the made-up header value");
		}

		[Test]
		public void TestSetRawValue ()
		{
			var header = new Header (HeaderId.Subject, "This is the subject");
			var rawValue = Encoding.ASCII.GetBytes ("This is the\n raw subject\n");
			var format = FormatOptions.Default.Clone ();
			format.International = true;

			header.SetRawValue (rawValue);

			var value = header.GetRawValue (format);
			Assert.AreEqual (rawValue.Length, value.Length, "Length");
			for (int i = 0; i < rawValue.Length; i++)
				Assert.AreEqual (rawValue[i], value[i], "rawValue[{0}]", i);
		}

		static string EncodeMailbox (FormatOptions options, string field, MailboxAddress mailbox)
		{
			var list = new InternetAddressList ();
			var builder = new StringBuilder (" ");
			int lineLength = field.Length;

			list.Add (mailbox);
			list.Encode (options, builder, true, ref lineLength);
			builder.Append (options.NewLine);

			return builder.ToString ();
		}

		static void TestReformatAddressHeader (FormatOptions options, MailboxAddress mailbox)
		{
			// encode the mailbox the way it would be encoded if it was added to MimeMessage.From
			var encoded = EncodeMailbox (FormatOptions.Default, "From: ", mailbox);
			var rawValue = Encoding.UTF8.GetBytes (encoded);
			var header = new Header (ParserOptions.Default, HeaderId.From, "From", rawValue);

			// reformat it the way it would be reformatted by MimeMessage.WriteTo()
			var result = Encoding.UTF8.GetString (header.GetRawValue (options));
			var expected = EncodeMailbox (options, "From: ", mailbox);

			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestReformatAddressHeaderWithInnerQuotedString ()
		{
			var mailbox = new MailboxAddress ("John \"Jacob Jingle Heimer\" Schmidt", "example@example.com");
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			TestReformatAddressHeader (options, mailbox);
		}

		[Test]
		public void TestReformatAddressHeaderWithInnerUnicodeQuotedString1 ()
		{
			var mailbox = new MailboxAddress ("John \"點看@名がドメイン Jacob Jingle Heimer\" Schmidt", "example@example.com");
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			TestReformatAddressHeader (options, mailbox);
		}

		[Test]
		public void TestReformatAddressHeaderWithInnerUnicodeQuotedString2 ()
		{
			var mailbox = new MailboxAddress ("John \"Jacob Jingle 點看@名がドメイン Heimer\" Schmidt", "example@example.com");
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			TestReformatAddressHeader (options, mailbox);
		}

		[Test]
		public void TestReformatAddressHeaderWithInnerUnicodeQuotedString3 ()
		{
			var mailbox = new MailboxAddress ("John \"Jacob Jingle Heimer 點看@名がドメイン\" Schmidt", "example@example.com");
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			TestReformatAddressHeader (options, mailbox);
		}

		[Test]
		public void TestReformatAddressHeaderWithInnerUnicodeComment ()
		{
			var mailbox = new MailboxAddress ("John (Jacob Jingle Heimer) Schmidt", "example@example.com");
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			TestReformatAddressHeader (options, mailbox);
		}

		[Test]
		public void TestReformatAddressHeaderWithInnerUnicodeComment1 ()
		{
			var mailbox = new MailboxAddress ("John (點看@名がドメイン Jacob Jingle Heimer) Schmidt", "example@example.com");
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			TestReformatAddressHeader (options, mailbox);
		}

		[Test]
		public void TestReformatAddressHeaderWithInnerUnicodeComment2 ()
		{
			var mailbox = new MailboxAddress ("John (Jacob Jingle 點看@名がドメイン Heimer) Schmidt", "example@example.com");
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			TestReformatAddressHeader (options, mailbox);
		}

		[Test]
		public void TestReformatAddressHeaderWithInnerUnicodeComment3 ()
		{
			var mailbox = new MailboxAddress ("John (Jacob Jingle Heimer 點看@名がドメイン) Schmidt", "example@example.com");
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			TestReformatAddressHeader (options, mailbox);
		}

		[Test]
		public void TestReformatAddressHeaderWithLongSentenceWithCommas ()
		{
			var mailbox = new MailboxAddress ("Once upon a time, back when things that are old now were new, there lived a man with a very particular set of skills.", "example@example.com");
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			TestReformatAddressHeader (options, mailbox);
		}

		[Test]
		public void TestReformatReceived ()
		{
			const string received = " from Messages.8.5.N.CUILIB.3.45.SNAP.NOT.LINKED.greenbush.galaxy.sun4.41\r\n          via MS.5.6.greenbush.galaxy.sun4_41;\r\n          Fri, 12 Jun 1992 13:29:05 -0400 (EDT)";
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			var rawValue = Encoding.UTF8.GetBytes (received);
			var header = new Header (ParserOptions.Default, HeaderId.Received, "Received", rawValue);

			// reformat it the way it would be reformatted by MimeMessage.WriteTo()
			var result = Encoding.UTF8.GetString (header.GetRawValue (options));

			Assert.AreEqual (received, result);
		}

		[Test]
		public void TestReformatContentId ()
		{
			const string contentId = "\r\n\t<id@example.com>\r\n";
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			var rawValue = Encoding.UTF8.GetBytes (contentId);
			var header = new Header (ParserOptions.Default, HeaderId.ContentId, "Content-Id", rawValue);

			// reformat it the way it would be reformatted by MimeMessage.WriteTo()
			var result = Encoding.UTF8.GetString (header.GetRawValue (options));

			Assert.AreEqual (contentId, result);
		}

		[Test]
		public void TestReformatReferences ()
		{
			const string references = "\r\n\t<id1@example.com>\r\n\t<id2@example.com>\r\n\t<id3@example.com>\r\n";
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			var rawValue = Encoding.UTF8.GetBytes (references);
			var header = new Header (ParserOptions.Default, HeaderId.References, "References", rawValue);

			// reformat it the way it would be reformatted by MimeMessage.WriteTo()
			var result = Encoding.UTF8.GetString (header.GetRawValue (options));

			Assert.AreEqual (references, result);
		}

		[Test]
		public void TestReformatContentDisposition ()
		{
			const string contentDisposition = " attachment; filename*=gb18030''%B2%E2%CA%D4%CE%C4%B1%BE.txt\r\n";
			const string expected = " attachment; filename=\"测试文本.txt\"\r\n";
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			var rawValue = Encoding.UTF8.GetBytes (contentDisposition);
			var header = new Header (ParserOptions.Default, HeaderId.ContentDisposition, "Content-Disposition", rawValue);

			// reformat it the way it would be reformatted by MimeMessage.WriteTo()
			var result = Encoding.UTF8.GetString (header.GetRawValue (options));

			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestReformatContentType ()
		{
			const string contentType = " text/plain; name*=gb18030''%B2%E2%CA%D4%CE%C4%B1%BE.txt\r\n";
			const string expected = " text/plain; name=\"测试文本.txt\"\r\n";
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			var rawValue = Encoding.UTF8.GetBytes (contentType);
			var header = new Header (ParserOptions.Default, HeaderId.ContentType, "Content-Type", rawValue);

			// reformat it the way it would be reformatted by MimeMessage.WriteTo()
			var result = Encoding.UTF8.GetString (header.GetRawValue (options));

			Assert.AreEqual (expected, result);
		}

		[Test]
		public void TestReformatAuthenticationResults ()
		{
			const string authenticationResults = " mx.google.com;\r\n       dkim=pass header.i=@example.com header.s=default header.b=sQFuh0qx;\r\n       spf=pass (google.com: domain of info@example.com designates 123.456.1.1 as permitted sender) smtp.mailfrom=info@example.com;\r\n       dmarc=pass (p=NONE sp=NONE dis=NONE) header.from=example.com\r\n";
			//const string expected = " mx.google.com;\r\n\tdkim=pass header.i=@example.com header.s=default header.b=sQFuh0qx; spf=pass\r\n\t(google.com: domain of info@example.com designates 123.456.1.1 as permitted sender)\r\n\tsmtp.mailfrom=info@example.com;\r\n\tdmarc=pass (p=NONE sp=NONE dis=NONE) header.from=example.com\r\n";
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			var rawValue = Encoding.UTF8.GetBytes (authenticationResults);
			var header = new Header (ParserOptions.Default, HeaderId.AuthenticationResults, "Authentication-Results", rawValue);

			// reformat it the way it would be reformatted by MimeMessage.WriteTo()
			var result = Encoding.UTF8.GetString (header.GetRawValue (options));

			Assert.AreEqual (authenticationResults, result);
		}

		[Test]
		public void TestReformatSubject ()
		{
			const string subject =  " I'm so happy! =?utf-8?b?5ZCN44GM44OJ44Oh44Kk44Oz?= I love MIME so\r\n much =?utf-8?b?4p2k77iP4oCN8J+UpSE=?= Isn't it great?\r\n";
			const string expected = " I'm so happy! 名がドメイン I love MIME so much ❤️‍🔥! Isn't it great?\r\n";
			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = true;

			var rawValue = Encoding.UTF8.GetBytes (subject);
			var header = new Header (ParserOptions.Default, HeaderId.Subject, "Subject", rawValue);

			// reformat it the way it would be reformatted by MimeMessage.WriteTo()
			var result = Encoding.UTF8.GetString (header.GetRawValue (options));

			Assert.AreEqual (expected, result);
		}

		// Note: These examples come from rfc2369
		[TestCase (HeaderId.ListHelp, "<mailto:list@host.com?subject=help> (List Instructions)", " <mailto:list@host.com?subject=help> (List Instructions)\r\n")]
		[TestCase (HeaderId.ListHelp, "<mailto:list-manager@host.com?body=info>", " <mailto:list-manager@host.com?body=info>\r\n")]
		[TestCase (HeaderId.ListHelp, "<mailto:list-info@host.com> (Info about the list)", " <mailto:list-info@host.com> (Info about the list)\r\n")]
		[TestCase (HeaderId.ListHelp, "<http://www.host.com/list/>, <mailto:list-info@host.com>", " <http://www.host.com/list/>, <mailto:list-info@host.com>\r\n")]
		[TestCase (HeaderId.ListHelp, "<ftp://ftp.host.com/list.txt> (FTP), <mailto:list@host.com?subject=help>", " <ftp://ftp.host.com/list.txt> (FTP),\r\n <mailto:list@host.com?subject=help>\r\n")]
		[TestCase (HeaderId.ListUnsubscribe, "<mailto:list@host.com?subject=unsubscribe>", " <mailto:list@host.com?subject=unsubscribe>\r\n")]
		[TestCase (HeaderId.ListUnsubscribe, "(Use this command to get off the list) <mailto:list-manager@host.com?body=unsubscribe%20list>", " (Use this command to get off the list)\r\n <mailto:list-manager@host.com?body=unsubscribe%20list>\r\n")]
		[TestCase (HeaderId.ListUnsubscribe, "<mailto:list-off@host.com>", " <mailto:list-off@host.com>\r\n")]
		[TestCase (HeaderId.ListUnsubscribe, "<http://www.host.com/list.cgi?cmd=unsub&lst=list>, <mailto:list-request@host.com?subject=unsubscribe>", " <http://www.host.com/list.cgi?cmd=unsub&lst=list>,\r\n <mailto:list-request@host.com?subject=unsubscribe>\r\n")]
		[TestCase (HeaderId.ListSubscribe, "<mailto:list@host.com?subject=subscribe>", " <mailto:list@host.com?subject=subscribe>\r\n")]
		[TestCase (HeaderId.ListSubscribe, "<mailto:list-request@host.com?subject=subscribe>", " <mailto:list-request@host.com?subject=subscribe>\r\n")]
		[TestCase (HeaderId.ListSubscribe, "(Use this command to join the list) <mailto:list-manager@host.com?body=subscribe%20list>", " (Use this command to join the list)\r\n <mailto:list-manager@host.com?body=subscribe%20list>\r\n")]
		[TestCase (HeaderId.ListSubscribe, "<mailto:list-on@host.com>", " <mailto:list-on@host.com>\r\n")]
		[TestCase (HeaderId.ListSubscribe, "<http://www.host.com/list.cgi?cmd=sub&lst=list>, <mailto:list-manager@host.com?body=subscribe%20list>", " <http://www.host.com/list.cgi?cmd=sub&lst=list>,\r\n <mailto:list-manager@host.com?body=subscribe%20list>\r\n")]
		[TestCase (HeaderId.ListPost, "<mailto:list@host.com>", " <mailto:list@host.com>\r\n")]
		[TestCase (HeaderId.ListPost, "<mailto:moderator@host.com> (Postings are Moderated)", " <mailto:moderator@host.com> (Postings are Moderated)\r\n")]
		[TestCase (HeaderId.ListPost, "<mailto:moderator@host.com?subject=list%20posting>", " <mailto:moderator@host.com?subject=list%20posting>\r\n")]
		[TestCase (HeaderId.ListPost, "NO (posting not allowed on this list)", " NO (posting not allowed on this list)\r\n")]
		[TestCase (HeaderId.ListOwner, "<mailto:listmom@host.com> (Contact Person for Help)", " <mailto:listmom@host.com> (Contact Person for Help)\r\n")]
		[TestCase (HeaderId.ListOwner, "<mailto:grant@foo.bar> (Grant Neufeld)", " <mailto:grant@foo.bar> (Grant Neufeld)\r\n")]
		[TestCase (HeaderId.ListOwner, "<mailto:josh@foo.bar?Subject=list>", " <mailto:josh@foo.bar?Subject=list>\r\n")]
		[TestCase (HeaderId.ListArchive, "<mailto:archive@host.com?subject=index%20list>", " <mailto:archive@host.com?subject=index%20list>\r\n")]
		[TestCase (HeaderId.ListArchive, "<ftp://ftp.host.com/pub/list/archive/>", " <ftp://ftp.host.com/pub/list/archive/>\r\n")]
		[TestCase (HeaderId.ListArchive, "<http://www.host.com/list/archive/> (Web Archive)", " <http://www.host.com/list/archive/> (Web Archive)\r\n")]
		// The following examples are meant to test unicode comments
		[TestCase (HeaderId.ListHelp, "<mailto:list@host.com?subject=help> (목록 지침)", " <mailto:list@host.com?subject=help>\r\n (=?utf-8?b?66qp66GdIOyngOy5qA==?=)\r\n", " <mailto:list@host.com?subject=help> (목록 지침)\r\n")]
		[TestCase (HeaderId.ListUnsubscribe, "(이 명령을 사용하여 목록에서 구독을 취소합니다.) <mailto:list-manager@host.com?body=unsubscribe%20list>", "\r\n (=?utf-8?b?7J20IOuqheugueydhCDsgqzsmqntlZjsl6wg66qp66Gd7JeQ7ISc?=\r\n =?utf-8?b?IOq1rOuPheydhCDst6jshoztlanri4jri6Qu?=)\r\n <mailto:list-manager@host.com?body=unsubscribe%20list>\r\n", " (이 명령을 사용하여 목록에서 구독을 취소합니다.)\r\n <mailto:list-manager@host.com?body=unsubscribe%20list>\r\n")]
		[TestCase (HeaderId.ListSubscribe, "(이 명령을 사용하여 목록에 조인합니다.) <mailto:list-manager@host.com?body=subscribe%20list>", " (=?utf-8?b?7J20IOuqheugueydhCDsgqzsmqntlZjsl6wg66qp66Gd7JeQ?=\r\n =?utf-8?b?IOyhsOyduO2VqeuLiOuLpC4=?=)\r\n <mailto:list-manager@host.com?body=subscribe%20list>\r\n", " (이 명령을 사용하여 목록에 조인합니다.)\r\n <mailto:list-manager@host.com?body=subscribe%20list>\r\n")]
		[TestCase (HeaderId.ListPost, "NO (이 목록에 게시가 허용되지 않음)", " NO\r\n (=?utf-8?b?7J20IOuqqeuhneyXkCDqsozsi5zqsIAg7ZeI7Jqp65CY7KeAIOyViuydjA==?=)\r\n", " NO (이 목록에 게시가 허용되지 않음)\r\n")]
		// The following examples are specially crafted to hit various corner cases that the above cases do not hit
		[TestCase (HeaderId.ListPost, "(This long comment should force the 'NO' token onto the next line) NO <mailto:list-manager@host.com>", " (This long comment should force the 'NO' token onto the next line)\r\n NO <mailto:list-manager@host.com>\r\n", " (This long comment should force the 'NO' token onto the next line)\r\n NO <mailto:list-manager@host.com>\r\n")]
		[TestCase (HeaderId.ListHelp, "This is a super-califragilistic-expialidociously-looooooooooooooooooooooooong-word-token that will need to be broken up <mailto:list-manager@host.com?subject=help>", " This is a super-califragilistic-expialidociously-looooooooooooooooo\r\n oooooooong-word-token that will need to be broken up\r\n <mailto:list-manager@host.com?subject=help>\r\n")]
		public void TestEncodeListCommandHeader (HeaderId id, string value, string expected, string international = null)
		{
			var header = new Header (id, value);

			var result = Encoding.UTF8.GetString (header.RawValue);

			Assert.AreEqual (expected.ReplaceLineEndings (), result, "RawValue");

			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = false;

			result = Encoding.UTF8.GetString (header.GetRawValue (options));

			Assert.AreEqual (expected.ReplaceLineEndings (), result, "GetRawValue");

			options.International = true;

			result = Encoding.UTF8.GetString (header.GetRawValue (options));

			Assert.AreEqual (international ?? expected, result, "GetRawValue International");
		}

		[Test]
		public void TestEncodeListCommandHeaderWithExtremelyLongUrl ()
		{
			const string value = "<https://www.some-link.com/query-params?abcd=efgh&this=is-very-long-string-which-should-not-be-Rfc2047-encoded-and-should-be-kept-the-way-it-is-by-default>";
			const string expected = "\r\n <https://www.some-link.com/query-params?abcd=efgh&this=is-very-long-string-which-should-not-be-Rfc2047-encoded-and-should-be-kept-the-way-it-is-by-default>\r\n";
			var header = new Header (HeaderId.ListUnsubscribe, value);

			var options = FormatOptions.Default.Clone ();
			options.NewLineFormat = NewLineFormat.Dos;
			options.International = false;

			var result = Encoding.UTF8.GetString (header.GetRawValue (options));

			Assert.AreEqual (expected.ReplaceLineEndings (), result);
		}
	}
}
