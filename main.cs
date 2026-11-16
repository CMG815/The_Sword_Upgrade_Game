using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnhancementSystem : MonoBehaviour
{
    [Header("강화 기본 설정")]
    public int 최대강화단계 = 25;
    public int 시작골드 = 150000;
    public int 기본강화비용 = 1000;

    [Header("확률 및 장인의 기운")]
    public float[] 기본성공확률 = new float[25];
    public float 최대장인의기운 = 100f;

    private int 현재강화단계 = 0;
    private int 현재골드;
    private float 장인의기운 = 0f;
    private int 최근강화비용 = 0;
    private int 판매금액 = 0;

    [Header("UI 요소")]
    public Text 강화상태텍스트;
    public Text 현재골드텍스트;
    public Text 강화확률텍스트;
    public Text 장인의기운텍스트;
    public Text 강화단계텍스트;
    public Text 강화골드텍스트;
    public Button 강화버튼;
    public Button 판매버튼;
    public Button 강화이동버튼;
    public Button 상점이동버튼;
    public Button 메인이동버튼;
    public Button 초기화버튼;
    public Button 시작화면버튼;
    public Button 다시시작버튼; // 새로 추가된 버튼
    public Button 게임종료버튼; // 게임 종료 버튼

    // 이미지 변경 관련
    [Header("강화 단계별 이미지")]
    public Image 아이템이미지;
    public Sprite[] 강화단계이미지; // 5강마다의 이미지를 여기에 할당 (0, 5, 10, 15, 20, 25)

    // 확률 증가 아이템 관련 UI 요소
    public Button 확률증가구매버튼;
    public Button 확률증가사용버튼;
    public Text 남은아이템개수텍스트;

    private int 남은아이템개수 = 0;    // 현재 소유한 확률 증가 아이템 개수
    public int 확률증가아이템가격 = 630000;  // 확률 증가 아이템 가격
    public float 확률증가량 = 0.015f;  // 확률 증가량
    private bool 확률증가사용중 = false;  // 확률 증가 아이템 사용 여부

    void Start()
    {
        // 강화 시스템 초기화
        if (강화버튼 != null) 강화버튼.onClick.AddListener(AttemptEnhancement);
        if (판매버튼 != null) 판매버튼.onClick.AddListener(SellItem);
        if (강화이동버튼 != null) 강화이동버튼.onClick.AddListener(MoveToSampleScene);
        if (상점이동버튼 != null) 상점이동버튼.onClick.AddListener(MoveToshop);
        if (메인이동버튼 != null) 메인이동버튼.onClick.AddListener(MoveTomain);
        if (시작화면버튼 != null) 시작화면버튼.onClick.AddListener(MoveTostart);
        if (초기화버튼 != null) 초기화버튼.onClick.AddListener(ResetSavedData);
        if (다시시작버튼 != null) 다시시작버튼.onClick.AddListener(RestartGame); // 다시 시작 버튼 리스너 추가
        if (게임종료버튼 != null) 게임종료버튼.onClick.AddListener(QuitGame); // 게임 종료 버튼 리스너 추가

        // 확률 증가 아이템 초기화
        if (확률증가구매버튼 != null) 확률증가구매버튼.onClick.AddListener(BuyEnhancementItem);
        if (확률증가사용버튼 != null) 확률증가사용버튼.onClick.AddListener(UseEnhancementItem);

        // 데이터 불러오기
        현재골드 = PlayerPrefs.GetInt("CurrentGold", 시작골드);
        현재강화단계 = PlayerPrefs.GetInt("CurrentEnhancementLevel", 0);
        장인의기운 = PlayerPrefs.GetFloat("CurrentArtisanEnergy", 0f);
        최근강화비용 = PlayerPrefs.GetInt("CurrentEnhancementCost", 0);
        남은아이템개수 = PlayerPrefs.GetInt("RemainingEnhanceItems", 0);

        // 기본 강화 확률 설정
        기본성공확률[0] = 0.50f; 기본성공확률[1] = 0.45f; 기본성공확률[2] = 0.40f;
        기본성공확률[3] = 0.35f; 기본성공확률[4] = 0.30f; 기본성공확률[5] = 0.25f;
        기본성공확률[6] = 0.20f; 기본성공확률[7] = 0.18f; 기본성공확률[8] = 0.16f;
        기본성공확률[9] = 0.14f; 기본성공확률[10] = 0.12f; 기본성공확률[11] = 0.10f;
        기본성공확률[12] = 0.08f; 기본성공확률[13] = 0.07f; 기본성공확률[14] = 0.06f;
        기본성공확률[15] = 0.05f; 기본성공확률[16] = 0.04f; 기본성공확률[17] = 0.03f;
        기본성공확률[18] = 0.025f; 기본성공확률[19] = 0.02f; 기본성공확률[20] = 0.015f;
        기본성공확률[21] = 0.015f; 기본성공확률[22] = 0.01f; 기본성공확률[23] = 0.0075f;
        기본성공확률[24] = 0.005f;

        UpdateUI();
    }

    // 강화 시도
    public void AttemptEnhancement()
    {
        if (현재강화단계 >= 최대강화단계)
        {
            Debug.Log("최대 강화 단계에 도달했습니다.");
            MoveToEndScene(); // 엔드 신으로 이동
            return;
        }

        int 강화비용금액 = CalculateEnhancementCost(현재강화단계);
        if (현재골드 < 강화비용금액)
        {
            Debug.Log("골드가 부족합니다.");
            return;
        }

        // 강화 확률 적용
        float 강화확률 = 기본성공확률[현재강화단계];
        if (확률증가사용중)
        {
            강화확률 += 확률증가량;  // 확률 증가량 적용
        }

        if (장인의기운 >= 최대장인의기운) 강화확률 = 1f;

        현재골드 -= 강화비용금액;

        if (Random.value <= 강화확률)
        {
            현재강화단계++;
            장인의기운 = 0f;
            Debug.Log("강화 성공!");
        }
        else
        {
            float 기운증가량 = 기본성공확률[현재강화단계] * 50f;
            장인의기운 += 기운증가량;
            장인의기운 = Mathf.Clamp(장인의기운, 0f, 최대장인의기운);
            Debug.Log("강화 실패, 장인의 기운이 증가했습니다.");
        }

        // 확률 증가 아이템 초기화 (성공/실패에 관계없이)
        확률증가사용중 = false;

        최근강화비용 = CalculateEnhancementCost(현재강화단계);
        UpdateUI();
    }

    // 강화 비용 계산
    private int CalculateEnhancementCost(int 단계)
    {
        float 강화비용비율 = 29112f / 25f;
        return Mathf.FloorToInt(기본강화비용 + (강화비용비율 * 단계));
    }

    // 확률 증가 아이템 구매
    private void BuyEnhancementItem()
    {
        if (현재골드 >= 확률증가아이템가격)
        {
            현재골드 -= 확률증가아이템가격;
            남은아이템개수++;
            PlayerPrefs.SetInt("CurrentGold", 현재골드);
            PlayerPrefs.SetInt("RemainingEnhanceItems", 남은아이템개수);
            UpdateUI();
        }
        else
        {
            Debug.Log("골드가 부족하여 확률 증가 아이템을 구매할 수 없습니다.");
        }
    }

    // 확률 증가 아이템 사용
    private void UseEnhancementItem()
    {
        if (남은아이템개수 > 0 && !확률증가사용중)
        {
            확률증가사용중 = true;  // 아이템 사용 상태로 설정
            남은아이템개수--;  // 아이템 개수 감소
            PlayerPrefs.SetInt("RemainingEnhanceItems", 남은아이템개수);
            Debug.Log("확률 증가 아이템을 사용했습니다! 다음 강화 시 확률이 증가합니다.");
            UpdateUI();
        }
        else if (확률증가사용중)
        {
            Debug.Log("이미 확률 증가 아이템이 적용되어 있습니다. 추가로 사용할 수 없습니다.");
        }
        else
        {
            Debug.Log("확률 증가 아이템이 부족합니다.");
        }
    }

    // 아이템 판매
    private void SellItem()
    {
        if (최근강화비용 == 0)
        {
            Debug.Log("판매할 아이템이 없습니다.");
            return;
        }

        if (현재강화단계 == 0)
        {
            Debug.Log("0강 아이템은 판매할 수 없습니다.");
            판매금액 = 0;
        }
        else
        {
            판매금액 = Mathf.FloorToInt(최근강화비용 * 0.3f * 100);
        }

        현재골드 += 판매금액;
        현재강화단계 = 0;
        장인의기운 = 0f;

        UpdateUI();
        Debug.Log($"아이템을 판매하여 {판매금액} 골드를 얻었습니다.");
    }

    // 강화 이동 (upScene으로 이동)
    private void MoveToSampleScene()
    {
        PlayerPrefs.SetInt("CurrentGold", 현재골드);
        PlayerPrefs.SetInt("CurrentEnhancementLevel", 현재강화단계);
        PlayerPrefs.SetFloat("CurrentArtisanEnergy", 장인의기운);

        SceneManager.LoadScene("upScene");
    }

    // 상점 이동 (shop으로 이동)
    private void MoveToshop()
    {
        // 현재 골드와 상태 저장
        PlayerPrefs.SetInt("CurrentGold", 현재골드);
        PlayerPrefs.SetInt("CurrentEnhancementLevel", 현재강화단계);
        PlayerPrefs.SetFloat("CurrentArtisanEnergy", 장인의기운);
        PlayerPrefs.SetInt("CurrentEnhancementCost", 최근강화비용);
        PlayerPrefs.SetInt("RemainingEnhanceItems", 남은아이템개수);

        // 상점 씬으로 이동
        SceneManager.LoadScene("shop");
    }

    // 메인 이동 (main으로 이동)
    private void MoveTomain()
    {
        PlayerPrefs.SetInt("CurrentGold", 현재골드);
        PlayerPrefs.SetInt("CurrentEnhancementLevel", 현재강화단계);
        PlayerPrefs.SetFloat("CurrentArtisanEnergy", 장인의기운);

        SceneManager.LoadScene("main");
    }

    // 시작 이동 (Start으로 이동)
    private void MoveTostart()
    {
        SceneManager.LoadScene("Start");
    }

    // 엔드 신으로 이동
    private void MoveToEndScene()
    {
        SceneManager.LoadScene("end");
    }

    // 저장 초기화 함수
    private void ResetSavedData()
    {
        PlayerPrefs.DeleteKey("CurrentGold");
        PlayerPrefs.DeleteKey("CurrentEnhancementLevel");
        PlayerPrefs.DeleteKey("CurrentArtisanEnergy");
        PlayerPrefs.DeleteKey("RemainingEnhanceItems");

        현재골드 = 시작골드;
        현재강화단계 = 0;
        장인의기운 = 0f;
        남은아이템개수 = 0;

        UpdateUI();
        Debug.Log("저장된 값이 초기화되었습니다.");
    }

    // 게임 다시 시작
    private void RestartGame()
    {
        현재골드 = 150000; // 골드를 15만으로 초기화
        현재강화단계 = 0;
        장인의기운 = 0f;
        남은아이템개수 = 0;

        PlayerPrefs.SetInt("CurrentGold", 현재골드);
        PlayerPrefs.SetInt("CurrentEnhancementLevel", 현재강화단계);
        PlayerPrefs.SetFloat("CurrentArtisanEnergy", 장인의기운);
        PlayerPrefs.SetInt("RemainingEnhanceItems", 남은아이템개수);

        SceneManager.LoadScene("Start");
    }

    // 게임 종료
    private void QuitGame()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }

    // UI 업데이트
    private void UpdateUI()
    {
        float 현재강화확률 = 기본성공확률[현재강화단계];
        if (확률증가사용중)
        {
            현재강화확률 += 확률증가량;  // UI에 표시할 확률 증가 적용
        }

        if (강화상태텍스트 != null) 강화상태텍스트.text = $"강화 +{현재강화단계}강";
        if (현재골드텍스트 != null) 현재골드텍스트.text = $"{현재골드}G";
        if (강화확률텍스트 != null) 강화확률텍스트.text = $"강화 확률: {현재강화확률 * 100}%";
        if (장인의기운텍스트 != null) 장인의기운텍스트.text = $"장인의 기운: {(장인의기운 / 최대장인의기운) * 100f}%";
        if (강화단계텍스트 != null) 강화단계텍스트.text = $"강화 단계: {현재강화단계}";
        if (강화골드텍스트 != null) 강화골드텍스트.text = $"강화 비용: {최근강화비용}G";
        if (남은아이템개수텍스트 != null) 남은아이템개수텍스트.text = $"남은 아이템: {남은아이템개수}개";

        // 이미지 변경 (5강마다)
        if (아이템이미지 != null && 강화단계이미지 != null)
        {
            int 이미지인덱스 = Mathf.Min(현재강화단계 / 5, 강화단계이미지.Length - 1);
            아이템이미지.sprite = 강화단계이미지[이미지인덱스];
        }
    }
}
