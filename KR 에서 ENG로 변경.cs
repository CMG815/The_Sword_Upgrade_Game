using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnhancementSystem : MonoBehaviour
{
    [Header("Enhancement Basic Settings")]
    // 기본 설정: 최대 강화 레벨, 시작 금액, 기본 강화 비용
    public int maxEnhancementLevel = 25;
    public int startGold = 150000;
    public int baseEnhancementCost = 1000;

    [Header("Probability and Artisan's Energy")]
    // 강화 확률과 장인 에너지 관련 변수
    public float[] baseSuccessRates = new float[25]; // 각 강화 레벨별 성공 확률
    public float maxArtisanEnergy = 100f; // 최대 장인 에너지

    private int currentEnhancementLevel = 0; // 현재 강화 레벨
    private int currentGold; // 현재 보유 금액
    private float artisanEnergy = 0f; // 현재 장인 에너지
    private int lastEnhancementCost = 0; // 마지막 강화 비용
    private int sellingPrice = 0; // 판매 가격

    [Header("UI Elements")]
    // UI 관련 변수들 (텍스트와 버튼들)
    public Text enhancementStatusText;
    public Text currentGoldText;
    public Text enhancementProbabilityText;
    public Text artisanEnergyText;
    public Text enhancementLevelText;
    public Text enhancementGoldText;
    public Button enhancementButton;
    public Button sellButton;
    public Button enhancementMoveButton;
    public Button shopMoveButton;
    public Button mainMoveButton;
    public Button resetButton;
    public Button startScreenButton;
    public Button restartButton; // 새로 추가된 재시작 버튼
    public Button quitGameButton; // 종료 버튼

    [Header("Enhancement Stage Images")]
    // 강화 단계별 이미지 설정
    public Image itemImage;
    public Sprite[] enhancementStageImages; // 5단계마다 해당 이미지 (0, 5, 10, 15, 20, 25)

    // 확률 증가 아이템 관련 UI 요소
    public Button probabilityIncreasePurchaseButton;
    public Button probabilityIncreaseUseButton;
    public Text remainingItemCountText;

    private int remainingItemCount = 0; // 보유한 확률 증가 아이템 수
    public int probabilityIncreaseItemPrice = 630000; // 확률 증가 아이템의 가격
    public float probabilityIncreaseAmount = 0.015f; // 확률 증가량
    private bool isProbabilityIncreaseUsed = false; // 확률 증가 아이템 사용 여부

    void Start()
    {
        // 강화 시스템 초기화
        if (enhancementButton != null) enhancementButton.onClick.AddListener(AttemptEnhancement);
        if (sellButton != null) sellButton.onClick.AddListener(SellItem);
        if (enhancementMoveButton != null) enhancementMoveButton.onClick.AddListener(MoveToSampleScene);
        if (shopMoveButton != null) shopMoveButton.onClick.AddListener(MoveToshop);
        if (mainMoveButton != null) mainMoveButton.onClick.AddListener(MoveTomain);
        if (startScreenButton != null) startScreenButton.onClick.AddListener(MoveTostart);
        if (resetButton != null) resetButton.onClick.AddListener(ResetSavedData);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame); // 재시작 버튼 리스너 추가
        if (quitGameButton != null) quitGameButton.onClick.AddListener(QuitGame); // 종료 버튼 리스너 추가

        // 확률 증가 아이템 초기화
        if (probabilityIncreasePurchaseButton != null) probabilityIncreasePurchaseButton.onClick.AddListener(BuyEnhancementItem);
        if (probabilityIncreaseUseButton != null) probabilityIncreaseUseButton.onClick.AddListener(UseEnhancementItem);

        // 저장된 데이터 불러오기
        currentGold = PlayerPrefs.GetInt("CurrentGold", startGold);
        currentEnhancementLevel = PlayerPrefs.GetInt("CurrentEnhancementLevel", 0);
        artisanEnergy = PlayerPrefs.GetFloat("CurrentArtisanEnergy", 0f);
        lastEnhancementCost = PlayerPrefs.GetInt("CurrentEnhancementCost", 0);
        remainingItemCount = PlayerPrefs.GetInt("RemainingEnhanceItems", 0);

        // 기본 성공 확률 설정
        baseSuccessRates[0] = 0.50f; baseSuccessRates[1] = 0.45f; baseSuccessRates[2] = 0.40f;
        baseSuccessRates[3] = 0.35f; baseSuccessRates[4] = 0.30f; baseSuccessRates[5] = 0.25f;
        baseSuccessRates[6] = 0.20f; baseSuccessRates[7] = 0.18f; baseSuccessRates[8] = 0.16f;
        baseSuccessRates[9] = 0.14f; baseSuccessRates[10] = 0.12f; baseSuccessRates[11] = 0.10f;
        baseSuccessRates[12] = 0.08f; baseSuccessRates[13] = 0.07f; baseSuccessRates[14] = 0.06f;
        baseSuccessRates[15] = 0.05f; baseSuccessRates[16] = 0.04f; baseSuccessRates[17] = 0.03f;
        baseSuccessRates[18] = 0.025f; baseSuccessRates[19] = 0.02f; baseSuccessRates[20] = 0.015f;
        baseSuccessRates[21] = 0.015f; baseSuccessRates[22] = 0.01f; baseSuccessRates[23] = 0.0075f;
        baseSuccessRates[24] = 0.005f;

        UpdateUI();
    }

    // 강화 시도 함수
    public void AttemptEnhancement()
    {
        if (currentEnhancementLevel >= maxEnhancementLevel)
        {
            Debug.Log("Maximum enhancement level reached.");
            MoveToEndScene(); // 최대 강화 레벨에 도달했을 때 엔드 씬으로 이동
            return;
        }

        // 강화 비용 계산
        int enhancementCost = CalculateEnhancementCost(currentEnhancementLevel);
        if (currentGold < enhancementCost)
        {
            Debug.Log("Not enough gold.");
            return;
        }

        // 강화 성공 확률 계산
        float enhancementProbability = baseSuccessRates[currentEnhancementLevel];
        if (isProbabilityIncreaseUsed)
        {
            enhancementProbability += probabilityIncreaseAmount;  // 확률 증가 아이템 사용시 확률 증가
        }

        // 장인 에너지가 최대라면 성공 확률 100%로 설정
        if (artisanEnergy >= maxArtisanEnergy) enhancementProbability = 1f;

        currentGold -= enhancementCost;

        // 강화 성공 여부 결정
        if (Random.value <= enhancementProbability)
        {
            currentEnhancementLevel++; // 성공 시 강화 레벨 증가
            artisanEnergy = 0f; // 강화 성공 후 장인 에너지 초기화
            Debug.Log("Enhancement successful!");
        }
        else
        {
            // 실패 시 장인 에너지 증가
            float energyIncreaseAmount = baseSuccessRates[currentEnhancementLevel] * 50f;
            artisanEnergy += energyIncreaseAmount;
            artisanEnergy = Mathf.Clamp(artisanEnergy, 0f, maxArtisanEnergy); // 장인 에너지 클램프
            Debug.Log("Enhancement failed, artisan's energy increased.");
        }

        // 확률 증가 아이템 사용 초기화
        isProbabilityIncreaseUsed = false;

        // 마지막 강화 비용 저장
        lastEnhancementCost = CalculateEnhancementCost(currentEnhancementLevel);
        UpdateUI();
    }

    // 강화 비용 계산 함수
    private int CalculateEnhancementCost(int level)
    {
        float enhancementCostRate = 29112f / 25f;
        return Mathf.FloorToInt(baseEnhancementCost + (enhancementCostRate * level));
    }

    // 강화 아이템 구매 함수
    private void BuyEnhancementItem()
    {
        if (currentGold >= probabilityIncreaseItemPrice)
        {
            currentGold -= probabilityIncreaseItemPrice; // 금액 차감
            remainingItemCount++; // 아이템 수 증가
            PlayerPrefs.SetInt("CurrentGold", currentGold); // 저장
            PlayerPrefs.SetInt("RemainingEnhanceItems", remainingItemCount);
            UpdateUI();
        }
        else
        {
            Debug.Log("Not enough gold to buy enhancement item.");
        }
    }

    // 강화 아이템 사용 함수
    private void UseEnhancementItem()
    {
        if (remainingItemCount > 0 && !isProbabilityIncreaseUsed)
        {
            isProbabilityIncreaseUsed = true;  // 아이템 사용 표시
            remainingItemCount--;  // 아이템 수 감소
            PlayerPrefs.SetInt("RemainingEnhanceItems", remainingItemCount);
            Debug.Log("Used enhancement probability increase item! Next enhancement will have increased probability.");
            UpdateUI();
        }
        else if (isProbabilityIncreaseUsed)
        {
            Debug.Log("Probability increase item already used. Cannot use again.");
        }
        else
        {
            Debug.Log("Not enough enhancement probability increase items.");
        }
    }

    // 아이템 판매 함수
    private void SellItem()
    {
        if (lastEnhancementCost == 0)
        {
            Debug.Log("No item to sell.");
            return;
        }

        sellingPrice = Mathf.FloorToInt(lastEnhancementCost * 0.5f); // 판매 가격은 마지막 강화 비용의 50%
        currentGold += sellingPrice;  // 금액 추가
        lastEnhancementCost = 0; // 마지막 강화 비용 초기화
        UpdateUI();
    }

    // 엔드 씬으로 이동하는 함수
    private void MoveToEndScene()
    {
        SceneManager.LoadScene("EndScene");
    }

    // 샵 씬으로 이동하는 함수
    private void MoveToshop()
    {
        PlayerPrefs.SetInt("CurrentGold", currentGold);
        PlayerPrefs.SetInt("RemainingEnhanceItems", remainingItemCount);
        PlayerPrefs.SetInt("CurrentEnhancementLevel", currentEnhancementLevel);
        PlayerPrefs.SetFloat("CurrentArtisanEnergy", artisanEnergy);
        SceneManager.LoadScene("shopScene");
    }

    // 메인 메뉴로 이동하는 함수
    private void MoveTomain()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // 시작 화면으로 이동하는 함수
    private void MoveTostart()
    {
        SceneManager.LoadScene("StartScreen");
    }

    // 저장된 데이터 초기화 함수
    private void ResetSavedData()
    {
        PlayerPrefs.DeleteAll(); // 모든 데이터 삭제
        currentGold = startGold; // 기본 금액으로 초기화
        currentEnhancementLevel = 0; // 강화 레벨 초기화
        artisanEnergy = 0f; // 장인 에너지 초기화
        remainingItemCount = 0; // 아이템 수 초기화
        UpdateUI(); // UI 업데이트
    }

    // UI 업데이트 함수
    private void UpdateUI()
    {
        // UI의 텍스트 요소들을 현재 상태로 업데이트
        if (enhancementStatusText != null) enhancementStatusText.text = $"Enhancement Level: {currentEnhancementLevel}";
        if (currentGoldText != null) currentGoldText.text = $"Gold: {currentGold}";
        if (enhancementProbabilityText != null) enhancementProbabilityText.text = $"Success Rate: {baseSuccessRates[currentEnhancementLevel] * 100}%";
        if (artisanEnergyText != null) artisanEnergyText.text = $"Artisan Energy: {artisanEnergy}/{maxArtisanEnergy}";
        if (enhancementLevelText != null) enhancementLevelText.text = $"Enhancement Level: {currentEnhancementLevel}";
        if (enhancementGoldText != null) enhancementGoldText.text = $"Enhancement Cost: {CalculateEnhancementCost(currentEnhancementLevel)}";
        if (remainingItemCountText != null) remainingItemCountText.text = $"Enhancement Items: {remainingItemCount}";

        // 강화 이미지 업데이트
        if (itemImage != null && enhancementStageImages != null && currentEnhancementLevel % 5 == 0)
        {
            int index = Mathf.FloorToInt(currentEnhancementLevel / 5f);
            if (index < enhancementStageImages.Length)
            {
                itemImage.sprite = enhancementStageImages[index];
            }
        }
    }

    // 게임 재시작 함수
    private void RestartGame()
    {
        PlayerPrefs.DeleteAll(); // 데이터 초기화
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);  // 현재 씬 리로드
    }

    // 게임 종료 함수
    private void QuitGame()
    {
        Application.Quit();  // 게임 종료
        Debug.Log("Game is quitting...");
    }
}
