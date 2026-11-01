<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output omit-xml-declaration="yes"/>

	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="WeaponDescription[@id='Javelin']/AvailablePieces">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
			<xsl:if test="not(AvailablePiece[@id='horse_bandits_javelin_blade'])">
				<AvailablePiece id="horse_bandits_javelin_blade"/>
			</xsl:if>
			<xsl:if test="not(AvailablePiece[@id='horse_bandits_javelin_blade_4'])">
				<AvailablePiece id="horse_bandits_javelin_blade_4"/>
			</xsl:if>
			<xsl:if test="not(AvailablePiece[@id='horse_bandits_simple_javelin_blade'])">
				<AvailablePiece id="horse_bandits_simple_javelin_blade"/>
			</xsl:if>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="WeaponDescription[@id='OneHandedPolearm_JavelinAlternative']/AvailablePieces">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
			<xsl:if test="not(AvailablePiece[@id='horse_bandits_javelin_blade'])">
				<AvailablePiece id="horse_bandits_javelin_blade"/>
			</xsl:if>
			<xsl:if test="not(AvailablePiece[@id='horse_bandits_javelin_blade_4'])">
				<AvailablePiece id="horse_bandits_javelin_blade_4"/>
			</xsl:if>
			<xsl:if test="not(AvailablePiece[@id='horse_bandits_simple_javelin_blade'])">
				<AvailablePiece id="horse_bandits_simple_javelin_blade"/>
			</xsl:if>
		</xsl:copy>
	</xsl:template>
</xsl:stylesheet>
