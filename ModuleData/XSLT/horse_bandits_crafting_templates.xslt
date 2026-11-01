<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output omit-xml-declaration="yes"/>

	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

	<!-- Raise stack cap so troop CraftedItems can keep up to 8 ammo. -->
	<xsl:template match="CraftingTemplate[@id='Javelin']/StatsData[@weapon_description='Javelin']/StatData[@stat_type='StackAmount']">
		<StatData
			stat_type="StackAmount"
			max_value="8"/>
	</xsl:template>

	<!-- Custom blades must be UsablePieces or CraftedItems load as null. -->
	<xsl:template match="CraftingTemplate[@id='Javelin']/UsablePieces">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
			<xsl:if test="not(UsablePiece[@piece_id='horse_bandits_javelin_blade'])">
				<UsablePiece piece_id="horse_bandits_javelin_blade"/>
			</xsl:if>
			<xsl:if test="not(UsablePiece[@piece_id='horse_bandits_javelin_blade_4'])">
				<UsablePiece piece_id="horse_bandits_javelin_blade_4"/>
			</xsl:if>
			<xsl:if test="not(UsablePiece[@piece_id='horse_bandits_simple_javelin_blade'])">
				<UsablePiece piece_id="horse_bandits_simple_javelin_blade"/>
			</xsl:if>
		</xsl:copy>
	</xsl:template>
</xsl:stylesheet>
