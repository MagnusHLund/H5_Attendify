import { Image } from '../../ui'
import { useTranslation } from '../../../lib/i18n'
import './Footer.scss'

export function Footer() {
  const { t } = useTranslation()

  return (
    <footer className="footer">
      <div className="footer__content">
        <div className="footer__project">
          <span className="footer__name">
            <Image
              src="/internal/logos/Attendify-small.png"
              alt={t('common.attendifyLogo')}
              className="footer__logo"
            />
            <span className="footer__name--blue">Attend</span>
            <span className="footer__name--green">ify</span>
          </span>
          <span className="footer__description">
            {t('footer.description')}
          </span>
        </div>
        <div className="footer__copyright">
          {t('footer.copyright', { year: new Date().getFullYear() })}
        </div>
        <a
          className="footer__github"
          href="https://github.com/MagnusHLund/H5_Attendify"
          target="_blank"
          rel="noopener noreferrer"
          aria-label={t('footer.github')}
        >
          <Image
            src="/external/github/Github.png"
            alt=""
            className="footer__github-icon"
          />
          <span>{t('footer.github')}</span>
        </a>
      </div>
    </footer>
  )
}
