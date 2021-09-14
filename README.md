# ITLec.Xml.Validation
ITLec.Xml.Validation


RegEx Examples:-

-ITLec:UniqueTagName
-ITLec:Between(1,999999999999)
-ITLec:Count(/REQUEST[n]/MESSAGE/ACCOUNT)
-ITLec:UniqueTagValue
-ITLec:Decimal(15,2)
-(?=^(?!.*[a-zA-Z]).*$)(?=\p{IsArabic})

- ITLec:SQLValidation(SELECT CASE WHEN (SELECT COUNT(*) FROM EntityData WHERE EntityXml.exist('(//*:BillerId[text()="{ITLec:XPath(//*[local-name()='BillerId'])}"])') = 1)>=1 THEN 'True' ELSE 'False' END AS IsValid) 

