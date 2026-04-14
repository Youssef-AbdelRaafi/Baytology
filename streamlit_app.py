"""
Baytology AI - Real Estate Explorer
-----------------------------------
This Streamlit application serves as the frontend for the Baytology Recommendation Engine.
It allows users to select a reference property and uses FAISS (Facebook AI Similarity Search)
to retrieve and display similar real estate listings based on vector embeddings.

Key Features:
- Real-time similarity search using pre-trained FAISS indices.
- Dynamic UI with custom CSS for high-contrast property cards.
- Interactive controls for recommendation quantity.
"""

import streamlit as st
import pandas as pd
import faiss
import numpy as np
import os

# --- إعدادات الصفحة ---
st.set_page_config(page_title="Baytology AI | مستكشف العقارات", layout="wide")

# --- تحسين التصميم (CSS) لضمان وضوح الألوان واستجابة الواجهة ---
def apply_custom_styles():
    st.markdown("""
        <style>
        @import url('https://fonts.googleapis.com/css2?family=Tajawal:wght@400;700&display=swap');
        html, body, [class*="css"], .stMarkdown {
            font-family: 'Tajawal', sans-serif;
            direction: rtl;
            text-align: right;
        }
        /* كارت العقار المرجعي */
        .reference-card {
            background-color: #f0f4f8;
            padding: 15px;
            border-right: 5px solid #1a73e8;
            border-radius: 10px;
            margin-bottom: 25px;
            color: #1a1a1a;
        }
        /* كروت الترشيحات المشابهة */
        .property-card {
            padding: 20px;
            border: 2px solid #e0e0e0;
            border-radius: 15px;
            background-color: #ffffff;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            margin-bottom: 20px;
            transition: transform 0.2s;
        }
        .property-card:hover {
            transform: translateY(-5px);
            border-color: #2e7d32;
        }
        .property-card p { color: #333333 !important; font-size: 15px; margin-bottom: 5px; }
        .property-card h4 { color: #2e7d32; font-weight: bold; margin-bottom: 15px; }
        .similarity-tag { color: #1565c0; font-weight: bold; font-size: 16px; }
        </style>
        """, unsafe_allow_html=True)

# --- تحميل الموارد ---
@st.cache_resource
def load_assets():
    try:
        BASE_DIR = os.path.dirname(os.path.abspath(__file__))
        index = faiss.read_index(os.path.join(BASE_DIR, 'faiss_index.bin'))
        master_df = pd.read_csv(os.path.join(BASE_DIR, 'Datasets', 'master_dataset.csv'))
        df_enc = pd.read_csv(os.path.join(BASE_DIR, 'Datasets', 'preprocessed_dataset.csv'))
        
        # التأكد من مطابقة أبعاد المتجهات للفهرس
        feature_vectors = df_enc.drop(columns=['id']).values.astype('float32')
        return index, master_df, feature_vectors, None
    except Exception as e:
        return None, None, None, str(e)

# --- تشغيل الواجهة ---
apply_custom_styles()
index, master_df, feature_vectors, error = load_assets()

if error:
    st.error(f"خطأ في تحميل البيانات: {error}")
    st.stop()

st.title("🏠 Baytology AI Explorer")
st.markdown("### نظام التوصيات الذكي للعقارات في السوق المصري")

# --- القائمة الجانبية (Sidebar) ---
with st.sidebar:
    st.header("🔍 إعدادات البحث")
    master_df['display_name'] = master_df['type'] + " (ID: " + master_df['id'].astype(str) + ")"
    
    # اختيار العقار المرجعي
    selected_name = st.selectbox("اختر العقار الذي تريد البحث عن شبيه له:", master_df['display_name'].tolist())
    
    # تحسين التحكم في عدد العقارات (استخدام Slider مع تحديد قيمة افتراضية)
    num_rec = st.slider("عدد النتائج المطلوبة:", min_value=3, max_value=15, value=6, step=3)
    
    st.divider()
    search_clicked = st.button("توليد التوصيات الذكية ✨", use_container_width=True)

# --- عرض البيانات ---
if selected_name:
    # الحصول على بيانات العقار المختار
    idx = master_df[master_df['display_name'] == selected_name].index[0]
    ref = master_df.iloc[idx]

    # تثبيت عرض مواصفات العقار المرجعي في الأعلى دائماً
    st.markdown(f"""
        <div class="reference-card">
            <h4>📍 المواصفات المرجعية الحالية:</h4>
            <p>نوع العقار: <b>{ref['type']}</b> | الموقع: <b>{ref['location']}</b></p>
        </div>
    """, unsafe_allow_html=True)
    
    m1, m2, m3 = st.columns(3)
    m1.metric("السعر المرجعي", f"{ref['price']:,} ج.م")
    m2.metric("المساحة", f"{ref['size']} م²")
    m3.metric("عدد الغرف", int(ref['bedrooms']))

    if search_clicked:
        st.divider()
        st.subheader("🤖 العقارات الأكثر تشابهاً:")

        # عملية البحث
        query_vec = feature_vectors[idx].reshape(1, -1)
        distances, indices = index.search(query_vec, num_rec + 1)

        # عرض النتائج في شبكة مرنة
        cols = st.columns(3)
        counter = 0
        
        for d, i in zip(distances[0], indices[0]):
            if i == idx: continue # تخطي العقار نفسه
            if counter >= num_rec: break

            res = master_df.iloc[i]
            # حساب نسبة التشابه (تنسيق بسيط للعرض)
            sim_score = round(max(0, 100 - (d * 5)), 1)

            with cols[counter % 3]:
                st.markdown(f"""
                <div class="property-card">
                    <h4>{res['type']}</h4>
                    <p><b>📍 الموقع:</b> {res['location']}</p>
                    <p><b>💰 السعر:</b> {res['price']:,} ج.م</p>
                    <p><b>📏 المساحة:</b> {res['size']} م²</p>
                    <p><b>🛏️ غرف:</b> {int(res['bedrooms'])} | <b>🚿 حمام:</b> {int(res['bathrooms'])}</p>
                    <hr style="border: 0.5px solid #eee;">
                    <p class="similarity-tag">✅ درجة التطابق: {sim_score}%</p>
                </div>
                """, unsafe_allow_html=True)
                counter += 1