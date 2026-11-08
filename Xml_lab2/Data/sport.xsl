<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" encoding="utf-8" indent="yes"/>

	<xsl:template match="/SportClub">
		<html>
			<head>
				<meta charset="utf-8"/>
				<title>
					<xsl:value-of select="@Name"/>
				</title>
				<style>
					body  { font-family: Segoe UI, sans-serif; margin: 30px; background: #f7f9fc; color: #222; }
					h1    { color: #1f4e79; }
					h2    { color: #2e75b6; margin-top: 35px; border-bottom: 2px solid #2e75b6; padding-bottom: 4px; }
					table { border-collapse: collapse; width: 100%; margin: 10px 0 20px; background: #fff; }
					th, td{ border: 1px solid #b4c6e7; padding: 6px 10px; text-align: left; }
					th    { background: #dfe8f5; }
					.info { margin: 4px 0; }
					.teacher { background: #fff7e6; }
				</style>
			</head>
			<body>
				<h1>
					<xsl:value-of select="@Name"/>
				</h1>
				<p class="info">
					Кількість секцій: <b>
						<xsl:value-of select="count(Section)"/>
					</b>,
					учасників: <b>
						<xsl:value-of select="count(Section/Members/Member)"/>
					</b>,
					запланованих змагань: <b>
						<xsl:value-of select="count(Section/Competitions/Competition)"/>
					</b>
				</p>

				<xsl:apply-templates select="Section"/>
			</body>
		</html>
	</xsl:template>
	<xsl:template match="Section">
		<h2>
			Секція: <xsl:value-of select="@Name"/>
		</h2>
		<p class="info">
			Тренер: <b>
				<xsl:value-of select="@Coach"/>
			</b>
		</p>
		<p class="info">
			Графік роботи: <xsl:value-of select="Schedule/@Days"/>,
			<xsl:value-of select="Schedule/@Time"/>
			(<xsl:value-of select="Schedule/@Place"/>)
		</p>

		<h3>Учасники</h3>
		<table>
			<tr>
				<th>№</th>
				<th>П.І.П.</th>
				<th>Факультет</th>
				<th>Статус</th>
				<th>Курс / Посада</th>
				<th>Розряд / Ступінь</th>
			</tr>
			<xsl:for-each select="Members/Member">
				<tr>
					<xsl:if test="@Role">
						<xsl:attribute name="class">teacher</xsl:attribute>
					</xsl:if>
					<td>
						<xsl:value-of select="position()"/>
					</td>
					<td>
						<xsl:value-of select="@Name"/>
					</td>
					<td>
						<xsl:value-of select="@Faculty"/>
					</td>
					<td>
						<xsl:choose>
							<xsl:when test="@Role">
								<xsl:value-of select="@Role"/>
							</xsl:when>
							<xsl:otherwise>Студент</xsl:otherwise>
						</xsl:choose>
					</td>
					<td>
						<xsl:choose>
							<xsl:when test="@Course">
								<xsl:value-of select="@Course"/> курс
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="@Position"/>
							</xsl:otherwise>
						</xsl:choose>
					</td>
					<td>
						<xsl:value-of select="@Rank"/>
						<xsl:value-of select="@Degree"/>
						<xsl:if test="not(@Rank) and not(@Degree)">—</xsl:if>
					</td>
				</tr>
			</xsl:for-each>
		</table>

		<h3>План змагань</h3>
		<table>
			<tr>
				<th>Назва</th>
				<th>Дата</th>
				<th>Місце</th>
			</tr>
			<xsl:for-each select="Competitions/Competition">
				<xsl:sort select="@Date"/>
				<tr>
					<td>
						<xsl:value-of select="@Name"/>
					</td>
					<td>
						<xsl:value-of select="@Date"/>
					</td>
					<td>
						<xsl:value-of select="@Place"/>
					</td>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:template>
</xsl:stylesheet>