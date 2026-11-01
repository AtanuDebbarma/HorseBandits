<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output omit-xml-declaration="yes"/>

	<!--
		Graft Horse Bandits Soldier upgrades onto vanilla Caravan Ambush troops.
		Also fix encyclopedia presentation:
		- vanilla marks both is_hidden_encyclopedia=true (blocks list + party right-click)
		- Ransacker lacks is_basic_troop, so FindUpgradeRootOf cannot root the cavalry tree
		Veteran's Respect still required for Bandit -> Soldier. No item upgrade_requires.

		Highwayman (mountain_bandits_chief) -> Vlandian Brigand Champion (not vanilla Champion).
		MCM can point Highwayman back at Vlandian Champion when that switch is off.
		This XSLT stays the default so the branch is on if that code does not run.
	-->
	<xsl:variable name="huntsmanTarget">NPCCharacter.horse_bandits_mounted_huntsman</xsl:variable>
	<xsl:variable name="banditHorsemanTarget">NPCCharacter.horse_bandits_imperial_bandit_horseman</xsl:variable>
	<xsl:variable name="brigandChampionTarget">NPCCharacter.horse_bandits_vlandian_brigand_champion</xsl:variable>

	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

	<!-- Mounted Pillager: unhide, keep/ensure basic troop, add Huntsman branch. -->
	<xsl:template match="NPCCharacter[@id='mounted_pillager']">
		<xsl:copy>
			<xsl:apply-templates select="@*[name()!='is_hidden_encyclopedia' and name()!='is_basic_troop']"/>
			<xsl:attribute name="is_basic_troop">true</xsl:attribute>
			<xsl:apply-templates select="node()[not(self::upgrade_targets)]"/>
			<upgrade_targets>
				<xsl:apply-templates select="upgrade_targets/upgrade_target[not(@id=$huntsmanTarget)]"/>
				<upgrade_target id="{$huntsmanTarget}"/>
			</upgrade_targets>
		</xsl:copy>
	</xsl:template>

	<!-- Mounted Ransacker: unhide, mark basic troop (tree root), drop horse gate, add Horseman branch. -->
	<xsl:template match="NPCCharacter[@id='mounted_ransacker']">
		<xsl:copy>
			<xsl:apply-templates select="@*[name()!='is_hidden_encyclopedia' and name()!='is_basic_troop' and name()!='upgrade_requires']"/>
			<xsl:attribute name="is_basic_troop">true</xsl:attribute>
			<xsl:apply-templates select="node()[not(self::upgrade_targets)]"/>
			<upgrade_targets>
				<xsl:apply-templates select="upgrade_targets/upgrade_target[not(@id=$banditHorsemanTarget)]"/>
				<upgrade_target id="{$banditHorsemanTarget}"/>
			</upgrade_targets>
		</xsl:copy>
	</xsl:template>

	<!--
		Highwayman: mark is_basic_troop so FindUpgradeRootOf can root Champion/Banner Knight
		(same encyclopedia fix as Mounted Ransacker), and replace vanilla Champion with Brigand Champion.
	-->
	<xsl:template match="NPCCharacter[@id='mountain_bandits_chief']">
		<xsl:copy>
			<xsl:apply-templates select="@*[name()!='is_basic_troop']"/>
			<xsl:attribute name="is_basic_troop">true</xsl:attribute>
			<xsl:apply-templates select="node()[not(self::upgrade_targets)]"/>
			<upgrade_targets>
				<xsl:for-each select="upgrade_targets/upgrade_target">
					<xsl:choose>
						<xsl:when test="@id='NPCCharacter.vlandian_champion'">
							<upgrade_target id="{$brigandChampionTarget}"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:copy-of select="."/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:for-each>
				<xsl:if test="not(upgrade_targets/upgrade_target[@id='NPCCharacter.vlandian_champion' or @id=$brigandChampionTarget])">
					<upgrade_target id="{$brigandChampionTarget}"/>
				</xsl:if>
			</upgrade_targets>
		</xsl:copy>
	</xsl:template>
</xsl:stylesheet>
