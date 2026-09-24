import { useTranslation } from '../../lib/i18n'
import type { PrivacyPolicyConfiguration } from './components/types/privacyPolicyConfiguration'
import './PrivacyPolicyPage.scss'

const privacyPolicyConfig: PrivacyPolicyConfiguration = {
  ControllerName:
    import.meta.env.VITE_PRIVACY_POLICY_CONTROLLER_NAME?.trim() ?? '',
  ControllerAddress:
    import.meta.env.VITE_PRIVACY_POLICY_CONTROLLER_ADDRESS?.trim() ?? '',
  ControllerEmail:
    import.meta.env.VITE_PRIVACY_POLICY_CONTROLLER_EMAIL?.trim() ?? '',
  DpoContact: import.meta.env.VITE_PRIVACY_POLICY_DPO_CONTACT?.trim() || null,
  AuthorityName:
    import.meta.env.VITE_PRIVACY_POLICY_AUTHORITY_NAME?.trim() ?? '',
  AuthorityUrl: import.meta.env.VITE_PRIVACY_POLICY_AUTHORITY_URL?.trim() ?? '',
}

export function PrivacyPolicyPage() {
  const { t } = useTranslation()

  return (
    <main className="privacy-policy">
      <article className="privacy-policy__content">
        <header className="privacy-policy__header">
          <h1>{t('privacy.title')}</h1>
          <p>{t('privacy.lastUpdated')}</p>
        </header>

        <section>
          <h2>{t('privacy.introduction.title')}</h2>
          <p>{t('privacy.introduction.text')}</p>
        </section>

        <section className="privacy-policy__controller">
          <h2>{t('privacy.controller.title')}</h2>
          <p className="privacy-policy__controller-intro">
            {t('privacy.controller.intro')}
          </p>

          <dl className="privacy-policy__controller-details">
            <div className="privacy-policy__controller-detail">
              <dt>{t('privacy.controller.name')}</dt>
              <dd>{privacyPolicyConfig.ControllerName}</dd>
            </div>

            <div className="privacy-policy__controller-detail">
              <dt>{t('privacy.controller.address')}</dt>
              <dd>{privacyPolicyConfig.ControllerAddress}</dd>
            </div>

            <div className="privacy-policy__controller-detail">
              <dt>{t('privacy.controller.email')}</dt>
              <dd>
                <a href={`mailto:${privacyPolicyConfig.ControllerEmail}`}>
                  {privacyPolicyConfig.ControllerEmail}
                </a>
              </dd>
            </div>

            {privacyPolicyConfig.DpoContact && (
              <div className="privacy-policy__controller-detail">
                <dt>{t('privacy.controller.dpo')}</dt>
                <dd>{privacyPolicyConfig.DpoContact}</dd>
              </div>
            )}
          </dl>
        </section>

        <section>
          <h2>{t('privacy.dataCollected.title')}</h2>

          <h3>{t('privacy.dataCollected.account')}</h3>
          <p>{t('privacy.dataCollected.accountDescription')}</p>

          <h3>{t('privacy.dataCollected.facial')}</h3>
          <p>{t('privacy.dataCollected.facialDescription')}</p>

          <h3>{t('privacy.dataCollected.attendance')}</h3>
          <p>{t('privacy.dataCollected.attendanceDescription')}</p>

          <h3>{t('privacy.dataCollected.telemetry')}</h3>
          <p>{t('privacy.dataCollected.telemetryDescription')}</p>
        </section>

        <section>
          <h2>{t('privacy.purposes.title')}</h2>
          <p>{t('privacy.purposes.text')}</p>
        </section>

        <section>
          <h2>{t('privacy.legalBasis.title')}</h2>
          <p>{t('privacy.legalBasis.text')}</p>
        </section>

        <section>
          <h2>{t('privacy.facialData.title')}</h2>
          <p>{t('privacy.facialData.text')}</p>
        </section>

        <section>
          <h2>{t('privacy.retention.title')}</h2>
          <p>{t('privacy.retention.text')}</p>
        </section>

        <section>
          <h2>{t('privacy.sharing.title')}</h2>
          <p>{t('privacy.sharing.text')}</p>
        </section>

        <section>
          <h2>{t('privacy.internationalTransfers.title')}</h2>
          <p>{t('privacy.internationalTransfers.text')}</p>
        </section>

        <section>
          <h2>{t('privacy.security.title')}</h2>
          <p>{t('privacy.security.text')}</p>
        </section>

        <section>
          <h2>{t('privacy.yourRights.title')}</h2>
          <p>{t('privacy.yourRights.intro')}</p>

          <ul>
            <li>{t('privacy.yourRights.access')}</li>
            <li>{t('privacy.yourRights.rectification')}</li>
            <li>{t('privacy.yourRights.erasure')}</li>
            <li>{t('privacy.yourRights.restriction')}</li>
            <li>{t('privacy.yourRights.objection')}</li>
            <li>{t('privacy.yourRights.portability')}</li>
          </ul>
        </section>

        <section>
          <h2>{t('privacy.requests.title')}</h2>
          <p>{t('privacy.requests.text')}</p>
        </section>

        <section>
          <h2>{t('privacy.complaints.title')}</h2>
          <p>{t('privacy.complaints.intro')}</p>

          {privacyPolicyConfig.AuthorityName &&
            privacyPolicyConfig.AuthorityUrl && (
              <a href={privacyPolicyConfig.AuthorityUrl}>
                {privacyPolicyConfig.AuthorityName}
              </a>
            )}
        </section>

        <section>
          <h2>{t('privacy.privacyByDesign.title')}</h2>
          <p>{t('privacy.privacyByDesign.text')}</p>
        </section>

        <section>
          <h2>{t('privacy.dpia.title')}</h2>
          <p>{t('privacy.dpia.text')}</p>
        </section>

        <section>
          <h2>{t('privacy.changes.title')}</h2>
          <p>{t('privacy.changes.text')}</p>
        </section>
      </article>
    </main>
  )
}
