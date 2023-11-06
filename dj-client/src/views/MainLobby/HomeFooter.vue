<template>
  <div class="footer-container">
    <div class="content">
      <ul id="footer-item-container">
        <li class="footer-item-detail">
          <img
            :src="require('../../assets/logo-web.png')"
            alt="Hình ảnh"
            style="float: left; width: 60px; height: 60px"
          />
          <div style="height: 60px; display: flex; align-items: center">
            <h2
              style="
                font-size: 20px;
                font-weight: 600;
                color: white;
                font: system-ui, 'Segoe UI', Roboto, Helvetica, Arial,
                  sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji',
                  'Segoe UI Symbol';
                margin-left: 8px;
              "
            >
              Code Không Khó
            </h2>
          </div>
          <ul id="footer-contact">
            <li>
              <font-awesome-icon icon="fa-solid fa-phone" color="white" />
              <span>+84 95516175</span>
            </li>
            <li>
              <font-awesome-icon icon="fa-solid fa-envelope" color="white" />
              <span>lethien202901@gmail.com</span>
            </li>
            <li>
              <font-awesome-icon
                icon="fa-solid fa-location-dot"
                color="white"
              />
              <span>136 Hồ Tùng Mậu - Cầu Diễn - Hà Nội</span>
            </li>
          </ul>
        </li>
        <li class="footer-item-detail tl-mode">
          <div
            style="
              height: 60px;
              display: flex;
              align-items: center;
              justify-content: center;
            "
          >
            <h3
              style="
                font-size: 20px;
                font-weight: 600;
                color: white;
                font: system-ui, 'Segoe UI', Roboto, Helvetica, Arial,
                  sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji',
                  'Segoe UI Symbol';
                margin-left: 8px;
                display: block;
              "
            >
              Tư vấn & Định hướng
            </h3>
          </div>
          <FormContact />
        </li>
        <li class="footer-item-detail tl-mode" style="text-align: center">
          <div
            style="
              height: 60px;
              display: flex;
              align-items: center;
              justify-content: center;
            "
          >
            <h6
              style="
                font-size: 20px;
                font-weight: 600;
                color: white;
                font: system-ui, 'Segoe UI', Roboto, Helvetica, Arial,
                  sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji',
                  'Segoe UI Symbol';
                margin-left: 8px;
              "
            >
              Kết nối
            </h6>
          </div>

          <a
            href="https://studio.youtube.com/channel/UCvhIhLNL7ZE4tM_tEQZK5nA"
            target="_blank"
          >
            <img
              :src="require('../../assets/youtube.png')"
              alt="Hình ảnh"
              class="footer-icon"
            />
          </a>
          <a href="https://github.com/duythien-206" target="_blank">
            <img
              :src="require('../../assets/github.png')"
              alt="Hình ảnh"
              class="footer-icon"
            />
          </a>
          <a href="#" target="_blank">
            <img
              :src="require('../../assets/tiktok.png')"
              alt="Hình ảnh"
              class="footer-icon"
            />
          </a>
        </li>
      </ul>
    </div>
  </div>
</template>

<script>
import FormContact from "./FormContact.vue";
export default {
  name: "HomeFooter",
  components: { FormContact },
};
</script>

<style lang="css" scoped>
.footer-container {
  background-color: #01579b;
  width: 100%;
  margin-top: 100px;
  display: inline-block;
}
.footer-container .content {
  margin-left: 15%;
  width: 70%;
  display: inline-block;
}
.footer-container .content #footer-item-container .footer-item-detail {
  width: 33.333%;
  float: left;
  margin-top: 60px;
}
#footer-item-container span {
  color: white;
  font-size: 20px;
  font-weight: 300;
}
#footer-item-container .footer-item-detail img {
  width: 60px;
}
.footer-item-detail #footer-contact {
  margin-top: 20px;
}
.footer-item-detail #footer-contact li svg {
  margin: 0 8px 0 8px;
}
.footer-item-detail .footer-icon {
  width: 48px !important;
  padding: 0 4px 0 4px;
}
@media screen and (max-width: 1023px) and (min-width: 740px) {
  .course-item {
    width: 50% !important;
  }
  .footer-item-detail {
    display: none !important;
  }
  .tl-mode {
    display: block !important;
    width: 50% !important;
  }
}
@media screen and (max-width: 739px) {
  .footer-item-detail {
    display: none !important;
  }

  .tl-mode {
    display: block !important;
    width: 100% !important;
  }

  .user-check {
    display: none !important;
  }
}
</style>
