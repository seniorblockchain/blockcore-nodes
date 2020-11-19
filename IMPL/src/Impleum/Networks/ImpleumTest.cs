using System;
using Blockcore.Base.Deployments;
using Blockcore.Consensus;
using Blockcore.Consensus.BlockInfo;
using Blockcore.Networks;
using Impleum.Networks.Policies;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;

namespace Impleum.Networks
{
   public class ImpleumTest : ImpleumMain
   {
      public ImpleumTest()
      {
         NetworkType = NetworkType.Testnet;

         Name = ImpleumSetup.Test.Name;
         CoinTicker = ImpleumSetup.Test.CoinTicker;
         Magic = ConversionTools.ConvertToUInt32(ImpleumSetup.Magic, true);
         RootFolderName = ImpleumSetup.Test.RootFolderName;
         DefaultPort = ImpleumSetup.Test.DefaultPort;
         DefaultRPCPort = ImpleumSetup.Test.DefaultRPCPort;
         DefaultAPIPort = ImpleumSetup.Test.DefaultAPIPort;

         var consensusFactory = new PosConsensusFactory();

         Block genesisBlock = CreateGenesisBlock(consensusFactory,
            ImpleumSetup.Test.GenesisTime,
            ImpleumSetup.Test.GenesisNonce,
            ImpleumSetup.Test.GenesisBits,
            ImpleumSetup.Test.GenesisVersion,
            ImpleumSetup.Test.GenesisReward,
            ImpleumSetup.GenesisText);

         Genesis = genesisBlock;

         // Taken from StratisX.
         var consensusOptions = new PosConsensusOptions
         {
            MaxBlockBaseSize = 1_000_000,
            MaxStandardVersion = 2,
            MaxStandardTxWeight = 100_000,
            MaxBlockSigopsCost = 20_000,
            MaxStandardTxSigopsCost = 20_000 / 5,
            WitnessScaleFactor = 4
         };

         var buriedDeployments = new BuriedDeploymentsArray
         {
            [BuriedDeployments.BIP34] = 0,
            [BuriedDeployments.BIP65] = 0,
            [BuriedDeployments.BIP66] = 0
         };

         Consensus = new Consensus(
             consensusFactory: consensusFactory,
             consensusOptions: consensusOptions,
             coinType: ImpleumSetup.CoinType,
             hashGenesisBlock: genesisBlock.GetHash(),
             subsidyHalvingInterval: 216171,
             majorityEnforceBlockUpgrade: 750,
             majorityRejectBlockOutdated: 950,
             majorityWindow: 1000,
             buriedDeployments: buriedDeployments,
             bip9Deployments: new NoBIP9Deployments(),
             bip34Hash: null,
             minerConfirmationWindow: 2016, // nPowTargetTimespan / nPowTargetSpacing
             maxReorgLength: 500,
             defaultAssumeValid: null,
             maxMoney: long.MaxValue,
             coinbaseMaturity: 10,
             premineHeight: 2,
             premineReward: Money.Coins(ImpleumSetup.PremineReward),
             proofOfWorkReward: Money.Coins(ImpleumSetup.PoWBlockReward),
             targetTimespan: TimeSpan.FromSeconds(14 * 24 * 60 * 60), // two weeks
             targetSpacing: ImpleumSetup.TargetSpacing,
             powAllowMinDifficultyBlocks: false,
             posNoRetargeting: false,
             powNoRetargeting: false,
             powLimit: new Target(new uint256("000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
             minimumChainWork: null,
             isProofOfStake: true,
             lastPowBlock: ImpleumSetup.LastPowBlock,
             proofOfStakeLimit: new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
             proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
             proofOfStakeReward: Money.Coins(ImpleumSetup.PoSBlockReward),
             proofOfStakeTimestampMask: ImpleumSetup.ProofOfStakeTimestampMask
         );

         Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (ImpleumSetup.RegTest.PubKeyAddress) };
         Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (ImpleumSetup.RegTest.ScriptAddress) };
         Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (239) };
         Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x35), (0x87), (0xCF) };
         Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x35), (0x83), (0x94) };
         // Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2b };
         Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 115 };

         Bech32Encoders = new Bech32Encoder[2];
         var encoder = new Bech32Encoder(ImpleumSetup.RegTest.CoinTicker.ToLowerInvariant());
         Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
         Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

         Checkpoints = ImpleumSetup.Test.Checkpoints;
         DNSSeeds = ImpleumSetup.Test.DNS;
         SeedNodes = ImpleumSetup.Test.Nodes;

         StandardScriptsRegistry = new ImpleumStandardScriptsRegistry();

         // 64 below should be changed to TargetSpacingSeconds when we move that field.
         Assert(DefaultBanTimeSeconds <= Consensus.MaxReorgLength * 64 / 2);

         Assert(Consensus.HashGenesisBlock == uint256.Parse(ImpleumSetup.Test.HashGenesisBlock));
         Assert(Genesis.Header.HashMerkleRoot == uint256.Parse(ImpleumSetup.Test.HashMerkleRoot));

         RegisterRules(Consensus);
         RegisterMempoolRules(Consensus);
      }
   }
}
