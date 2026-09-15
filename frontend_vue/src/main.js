import { createApp } from 'vue'
import 'vuetify/styles'
import '@mdi/font/css/materialdesignicons.css'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import App from './App.vue'
import router from './router'
import { COMMUNITY_THEMES, initialTheme, saveTheme } from './theme'
import './styles.css'

const themeName = initialTheme()
saveTheme(themeName)

const vuetify = createVuetify({
  components,
  directives,
  theme: { defaultTheme: themeName, themes: COMMUNITY_THEMES },
  defaults: {
    VBtn: { rounded: 'lg' },
    VCard: { rounded: 'lg' },
    VTextField: { variant: 'outlined', density: 'comfortable' },
    VTextarea: { variant: 'outlined', density: 'comfortable' },
    VSelect: { variant: 'outlined', density: 'comfortable' }
  }
})

createApp(App).use(router).use(vuetify).mount('#app')
